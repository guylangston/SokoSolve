using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SokoSolve.Core.Solver.Lookup
{
    public class NodeLookupDoubleBuffered : INodeLookup
    {
        const         int IncomingBufferSize = 1_000;
        const         int LastIndex          = IncomingBufferSize - 1;
        private const int WaitStepTime       = 10;
        private readonly INodeLookup inner;
        private volatile int               bufferIndex = -1; // inc called so first will be -1 + 1 = 0
        private volatile bool              bufferLock;
        private volatile SolverNode[]      buffer    = new SolverNode[IncomingBufferSize];
        private volatile SolverNode[]      bufferAlt = new SolverNode[IncomingBufferSize];

        public NodeLookupDoubleBuffered(INodeLookup inner)
        {
            this.inner = inner;
        }

        public enum FlushMode
        {
            InsideLock,
            AfterLock,
            BackgroundTask
        }


        public FlushMode FlushMethod { get; set; } = FlushMode.InsideLock;
        public bool      UseBatching { get; set; }
        
        public bool IsThreadSafe => true;
        
        public SolverStatistics Statistics => inner.Statistics;
        public string TypeDescriptor => $"DoubleBuffer:bb[{IncomingBufferSize}:{(UseBatching ? "batched" : "single")}] ==> {inner.TypeDescriptor}";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => null;

        public void Add(SolverNode node)
        {
            Debug.Assert(node != null);
            if(bufferLock) CheckBufferLock();
            AddInner(node);
        }

        // NOTE: This is important as the majority of cases are via this method
        public void Add(IReadOnlyCollection<SolverNode> nodes)
        {
            // PreCheck
            Debug.Assert(nodes != null);
            Debug.Assert(nodes.All(x=>x != null));
            if (nodes.Count == 0) return;
            
            if (UseBatching)
            {
                if(bufferLock) CheckBufferLock();

                // Advance
                var b = Interlocked.Add(ref bufferIndex, nodes.Count);
                if (b <= LastIndex)
                {
                    var cc = 0;
                    foreach (var n in nodes)
                    {
                        buffer[b - cc] = n;
                        cc++;
                    }
                    Statistics.TotalNodes += nodes.Count;
                }
                else if (b > LastIndex)
                {
                    var start = b - nodes.Count + 1;
                    var insideCount = start <= LastIndex ? (LastIndex - start + 1) : 0;
                    
                    // Partial?
                    if (insideCount > 0)
                    {
                       partial = true;
                        // Split and push the remainder
                        var inside = nodes.Take(insideCount);
                        var cc = 0;
                        foreach (var n in inside)
                        {
                            buffer[LastIndex - cc] = n;
                            cc++;
                        }
                        Statistics.TotalNodes += insideCount;
                        
                        SwapBuffer();
                        
                        Add(nodes.Skip(insideCount).ToArray()); // CAREFUL: Stack Overflow?
                        partial = false;
                    }
                    else // No overlap so try again after swap
                    {
                        Thread.Sleep(20); // Time for partial to 

                        if (b > LastIndex * 2)
                        {
                            throw new Exception("Buffer Overflow");
                        }
                        
                        var state_bufferlock = bufferLock;
                        var state_partial = partial;
                        var state_flushing = flushing;
                        // Dont call Statistics.TotalNodes++; as it is inc in the recursize Add
                        Add(nodes);    // CAREFUL: Stack Overflow?
                    }
                }
            }
            else
            {
                foreach (var n in nodes)
                {
                    Add(n);
                }
            }
        }
        
        void AddInner(SolverNode node)
        {
            Debug.Assert(node != null);
            var b = Interlocked.Increment(ref bufferIndex);
            if (b < IncomingBufferSize-1)
            {
                buffer[b] = node;
                Statistics.TotalNodes++;
            }
            else if (b == IncomingBufferSize-1)
            {
                buffer[b]  = node;
                Statistics.TotalNodes++;
                
                SwapBuffer();
            }
            else if (b >= IncomingBufferSize)
            {
                // Dont call Statistics.TotalNodes++; as it is inc in the recursize Add
                Add(node);        
            }
        }

        private volatile bool partial;
        private volatile bool flushing;
        private readonly object locker = new object();
        void SwapBuffer()
        {
            bufferLock = true;
            var incommingBuffer = buffer;
            lock (locker)
            {
                if (!bufferLock) throw new InvalidAsynchronousStateException();
                while (flushing)
                {
                    // NOP
                }

                buffer      = bufferAlt;
                bufferAlt   = incommingBuffer;
                bufferIndex = -1;    // inc called so first will be -1 + 1 = 0
                
                Array.Fill(buffer, null);
                
                bufferLock  = false; // Using an alternative buffer, to allow FindMatch to finish on another thread

                if (FlushMethod == FlushMode.InsideLock)
                {
                    FlushToInner(incommingBuffer);    
                }
            }
            
            if (FlushMethod == FlushMode.InsideLock)
            {
                FlushToInner(incommingBuffer);    
            }
            else if (FlushMethod == FlushMode.BackgroundTask)
            {
                Task.Run(()=>FlushToInner(incommingBuffer));
            }
           
        }
        private void FlushToInner(SolverNode[] incommingBuffer)
        {
            flushing = true;
            foreach (var n in incommingBuffer)// Buffer may not be complete
            {
                if (n != null) inner.Add(n);
            }
            flushing = false;
        }

        public SolverNode? FindMatch(SolverNode find)
        {
            if (TryFindInBuffer(find, out var findMatch)) return findMatch;

            return inner.FindMatch(find);
            
            bool TryFindInBuffer(SolverNode findNode, out SolverNode result)
            {
                if(bufferLock) CheckBufferLock();  // TODO: I don't think this safe, the buffers may be swapped while still checking...

                var tempBuffer = buffer;
                var tempIndex  = Math.Min(bufferIndex, tempBuffer.Length-1);
            
                for (var cc = 0; cc < tempIndex; cc++)
                {
                    var v = tempBuffer[cc];
                    if ( v != null &&  findNode.CompareTo(v) == 0)
                    {
                        result = v;
                        return true;
                    }
                }

                result = default;
                return false;
            }
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckBufferLock()
        {
            while (bufferLock)
            {
                lock (locker)
                {
                    var NOP = 1; // Release imediately
                }
                //Thread.Sleep(WaitStepTime);
            }
        }

        
       

       
    }
}