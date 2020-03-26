using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeLookupThreadSafeBuffer : ISolverNodeLookup
    {
        const int IncomingBufferSize = 2048;
       
        private volatile int bufferIndex;
        private volatile bool bufferLock;
        private volatile SolverNode[] buffer = new SolverNode[IncomingBufferSize];
        private volatile SolverNode[] bufferAlt = new SolverNode[IncomingBufferSize];
        private readonly object locker = new object();
        private readonly LongTermSortedBlocks longTerm = new LongTermSortedBlocks();

        public SolverNodeLookupThreadSafeBuffer()
        {
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
        }
        
        public SolverStatistics Statistics { get; }

        public bool TrySample(out SolverNode? node)
        {
            var b = bufferIndex;
            if (b > 0 && b < IncomingBufferSize)
            {
                node = buffer[b];
                return true;
            }
            
            // Todo Sample from the sorted list

            node = null;
            return false;
        }

        public void Add(SolverNode node)
        {
            while (bufferLock) {  }

            var b = Interlocked.Increment(ref bufferIndex);

            if (b < IncomingBufferSize-1)
            {
                buffer[b] = node;
                Statistics.TotalNodes++;
            }
            else if (b == IncomingBufferSize-1)
            {
                bufferLock = true;
                
                buffer[b] = node;
                Statistics.TotalNodes++;
                
                // Use an alternative buffer, to allow FindMatch to finish on another thread
                var c = buffer;
                buffer      = bufferAlt;
                bufferAlt   = c;
                bufferIndex = 0;
                
                lock (locker)
                {
                    longTerm.FlushBufferToSorted(c);
                }
                bufferLock = false;
            }
            else if (b >= IncomingBufferSize)
            {
                // Unlikely concurrency issue: try again
                while (bufferLock) { Thread.Sleep(10); }
                Add(node);
            }
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
            
            
            // Search buffer
            if (TryFindInBuffer(find, out var findMatch)) return findMatch;

            while (bufferLock) { Thread.Sleep(5); }

            return longTerm.FindMatch(find);
        }

        private bool TryFindInBuffer(SolverNode find, out SolverNode findMatch)
        {
            while (bufferLock) { Thread.Sleep(5); }
            
            var tempBuffer = buffer;
            var tempIndex  = Math.Min(bufferIndex, tempBuffer.Length-1);
            
            for (int cc = 0; cc < tempIndex; cc++)
            {
                var item = tempBuffer[cc];
                if (item != null && item.CompareTo(find) == 0)
                {
                    findMatch = item;
                    return true;
                }
            }

            findMatch = null;
            return false;
        }

        public IEnumerable<SolverNode> GetAll()
        {
            foreach (var n in buffer)
            {
                if (n != null) yield return n;
            }

            foreach (var n in longTerm.GetAll())
            {
                if (n != null) yield return n;
            }
        }

        class LongTermSortedBlocks
        {
            
            LongTermBlock current = new LongTermBlock();
            List<LongTermBlock> frozenBlocks = new List<LongTermBlock>();
            
            public void FlushBufferToSorted(SolverNode[] buffer)
            {
                Array.Sort(buffer);
                current.Add(buffer);
                
                if (current.Count >= LongTermBlock.SortedBlockSize)
                {
                    frozenBlocks.Add(current);
                    current = new LongTermBlock();
                }
            }

            public SolverNode? FindMatch(SolverNode node)
            {
                var m = current.FindMatch(node);
                if (m != null) return m;
                
                foreach (var block in frozenBlocks)
                {
                    m = block.FindMatch(node);
                    if (m != null) return m;
                }

                return null;
            }

            public IEnumerable<SolverNode> GetAll()
            {
                foreach (var n in current.GetAll())
                {
                    if (n != null) yield return n;
                }

                foreach (var block in frozenBlocks)
                {
                    foreach (var n in block.GetAll())
                    {
                        if (n != null) yield return n;
                    }    
                }
                
            }
        }
        
        class LongTermBlock
        {
            public const int SortedBlockSize = 100_000;
            private List<SolverNode> block = new List<SolverNode>(SortedBlockSize);

            public int Count => block.Count;
            
            public void Add(SolverNode[] solverNodes)
            {
                block.AddRange(solverNodes);
                block.Sort();
            }

            public SolverNode? FindMatch(SolverNode node)
            {
                var i = block.BinarySearch(node);
                if (i < 0) return null;
                return block[i];
            }
            
            public IEnumerable<SolverNode> GetAll()
            {
                foreach (var b in block)
                {
                    if (b != null) yield return b;
                }   
            }
        }

    }
    
   
}