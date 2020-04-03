using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeLookupBufferedConcurrentLinkedList : ISolverNodeLookup
    {
        const int IncomingBufferSize = 2048;
       
        private volatile int                  bufferIndex;
        private volatile bool                 bufferLock;
        private volatile SolverNode[]         buffer    = new SolverNode[IncomingBufferSize];
        private volatile SolverNode[]         bufferAlt = new SolverNode[IncomingBufferSize];
        private readonly ReaderWriterLockSlim slimLock  = new ReaderWriterLockSlim();
        private readonly LongTermSortedBlocks longTerm  = new LongTermSortedBlocks();

        public SolverNodeLookupBufferedConcurrentLinkedList()
        {
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
        }
        
        public SolverStatistics Statistics { get; }

        public SolverNode? FindMatch(SolverNode find)
        {
            if (TryFindInBuffer(find, out var findMatch)) return findMatch;
            
            var ll =  longTerm.FindMatchFrozen(find);
            if (ll != null) return null;
            
            return longTerm.FindMatchCurrent(find);
        }

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
                bufferLock = true;
                
                slimLock.EnterWriteLock();;
                try
                {
                    buffer[b] = node;
                    
                    var c = buffer;
                    buffer      = bufferAlt;
                    bufferAlt   = c;
                    bufferIndex = 0;
                    bufferLock  = false; // Using an alternative buffer, to allow FindMatch to finish on another thread
                    
                    longTerm.FlushBufferToSorted(c);
                }
                finally
                {
                    slimLock.ExitWriteLock();
                }
            }
            else if (b >= IncomingBufferSize)
            {
                // Unlikely concurrency issue: try again
                Thread.Sleep(20);
                Add(node);
            }
            
            Statistics.TotalNodes++;
        }

        public void Add(IReadOnlyCollection<SolverNode> nodes)
        {
            foreach (var n in nodes)
            {
                Add(n);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckBufferLock()
        {
            while (bufferLock)
            {
                Thread.Sleep(10);
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


        class LongTermSortedBlocks
        {
            LinkedList<SolverNode> current = new LinkedList<SolverNode>();
            ConcurrentBag<LongTermBlock> frozenBlocks = new ConcurrentBag<LongTermBlock>();
            
            public void FlushBufferToSorted(SolverNode[] buffer)
            {
                foreach (var n in buffer)
                {
                    current.InsertSorted(n,(a, b) => SolverNode.ComparerInstance.Compare(a, b));
                }
                
                if (current.Count >= LongTermBlock.SortedBlockSize)
                {
                    frozenBlocks.Add(new LongTermBlock(current.ToArray()));
                }
            }

            public SolverNode? FindMatchCurrent(SolverNode node)
            {
                return current.FindInSorted(node, (a, b) => SolverNode.ComparerInstance.Compare(a, b));

            }

            public SolverNode? FindMatchFrozen(SolverNode node)
            {
                foreach (var block in frozenBlocks)
                {
                    var m = block.FindMatch(node);
                    if (m != null) return m;
                }

                return null;
            }

            public IEnumerable<SolverNode> GetAll()
            {
                foreach (var n in current)
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
            public const int SortedBlockSize = 200_000;
            private readonly SolverNode[] block;

            public LongTermBlock(SolverNode[] block)
            {
                Debug.Assert(ListHelper.IsSorted(block, SolverNode.ComparerInstance));
                this.block = block;
            }

            public int Count => block.Length;
            
            
            public SolverNode? FindMatch(SolverNode node)
            {
                var i =  Array.BinarySearch(block, node, SolverNode.ComparerInstance);
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