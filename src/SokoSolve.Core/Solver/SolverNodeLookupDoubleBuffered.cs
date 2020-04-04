using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeLookupDoubleBuffered : ISolverNodeLookup
    {
        private readonly ISolverNodeLookup inner;
        private volatile int               bufferIndex;
        private volatile bool              bufferLock;
        private volatile SolverNode[]      buffer             = new SolverNode[IncomingBufferSize];
        private volatile SolverNode[]      bufferAlt          = new SolverNode[IncomingBufferSize];
        const            int               IncomingBufferSize = 2048;
        private const int WaitStepTime = 10;
        

        public SolverNodeLookupDoubleBuffered(ISolverNodeLookup inner)
        {
            this.inner = inner;
        }
        
        public SolverStatistics Statistics => inner.Statistics;
        public string                                  GetTypeDescriptor                                 => null;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverCommandResult state) => throw new NotSupportedException();

        public void Add(SolverNode node)
        {
            CheckBufferLock();
            var b = Interlocked.Increment(ref bufferIndex);
            if (b < IncomingBufferSize-1)
            {
                buffer[b] = node;
            }
            else if (b == IncomingBufferSize-1)
            {
                AddAndSwapBuffer(node, b);
            }
            else if (b >= IncomingBufferSize)
            {
                // Unlikely concurrency issue: try again
                Thread.Sleep(WaitStepTime);
                Add(node);
            }
            
            Statistics.TotalNodes++;
        }

        
        protected virtual void AddAndSwapBuffer(SolverNode node, int b)
        {
            if (bufferLock) throw new InvalidAsynchronousStateException();
            bufferLock = true;
            buffer[b] = node;

            var c = buffer;
            buffer      = bufferAlt;
            bufferAlt   = c;
            bufferIndex = 0;
            bufferLock  = false; // Using an alternative buffer, to allow FindMatch to finish on another thread
            
            AddInner(c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void AddInner(IReadOnlyCollection<SolverNode> node)
        {
            inner.Add(node);
        }

        public void Add(IReadOnlyCollection<SolverNode> nodes)
        {
            foreach (var n in nodes)
            {
                Add(n);
            }
        }

        
        public SolverNode? FindMatch(SolverNode find)
        {
            if (TryFindInBuffer(find, out var findMatch)) return findMatch;

            return inner.FindMatch(find);
        }

        public IEnumerable<SolverNode> GetAll()
        {
            foreach (var n in buffer)
            {
                if (n != null) yield return n;
            }

            foreach (var n in inner.GetAll())
            {
                if (n != null) yield return n;
            }
        }
        
        public bool TrySample(out SolverNode? node)
        {
            var b = bufferIndex;
            if (b > 0 && b < IncomingBufferSize)
            {
                node = buffer[b];
                return true;
            }

            return inner.TrySample(out node);
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