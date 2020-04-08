using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SokoSolve.Core.Solver
{
    public class SolverPoolDoubleBuffered : ISolverPool
    {
        const         int IncomingBufferSize = 1_000;
        const         int LastIndex          = IncomingBufferSize - 1;
        private const int WaitStepTime       = 10;
        private readonly ISolverPool inner;
        private volatile int               bufferIndex = -1; // inc called so first will be -1 + 1 = 0
        private volatile bool              bufferLock;
        private volatile SolverNode[]      buffer    = new SolverNode[IncomingBufferSize];
        private volatile SolverNode[]      bufferAlt = new SolverNode[IncomingBufferSize];

        public SolverPoolDoubleBuffered(ISolverPool inner)
        {
            this.inner = inner;
        }
        
        public bool UseBatching { get; set; }
        
        public SolverStatistics Statistics => inner.Statistics;
        public string TypeDescriptor => $"DoubleBuffer:bb[{IncomingBufferSize}:{(UseBatching ? "batched" : "single")}] ==> {inner.TypeDescriptor}";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state) => null;

        public void Add(SolverNode node)
        {
            Debug.Assert(node != null);
            CheckBufferLock();
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
                CheckBufferLock();

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
        object locker = new object();
        void SwapBuffer()
        {
            bufferLock = true;
            lock (locker)
            {
                if (!bufferLock) throw new InvalidAsynchronousStateException();

                var incommingBuffer = buffer;
                buffer      = bufferAlt;
                bufferAlt   = incommingBuffer;
                bufferIndex = -1;    // inc called so first will be -1 + 1 = 0
                //#if DEBUG
                //Array.Fill(buffer, null);
                //#endif

                bufferLock  = false; // Using an alternative buffer, to allow FindMatch to finish on another thread
            
                Debug.Assert(incommingBuffer.All(x=>x != null));

                flushing = true;
                inner.Add(incommingBuffer);
                flushing = false;
            }
           
        }

        public SolverNode? FindMatch(SolverNode find)
        {
            if (TryFindInBuffer(find, out var findMatch)) return findMatch;

            return inner.FindMatch(find);
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckBufferLock()
        {
            while (bufferLock)
            {
                Thread.Sleep(WaitStepTime);
            }
        }

        
        private bool TryFindInBuffer(SolverNode find, out SolverNode findMatch)
        {
            CheckBufferLock();

            var tempBuffer = buffer;
            var tempIndex  = Math.Min(bufferIndex, tempBuffer.Length-1);
            
            for (var cc = 0; cc < tempIndex; cc++)
            {
                if (find.CompareTo(tempBuffer[cc]) == 0)
                {
                    findMatch = tempBuffer[cc];
                    return true;
                }
            }

            findMatch = null;
            return false;
        }

       
    }
}