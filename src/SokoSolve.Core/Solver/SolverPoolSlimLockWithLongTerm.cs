using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SokoSolve.Core.Solver
{
    public class NodeLookupSlimLockWithLongTerm : INodeLookup
    {
        private readonly ReaderWriterLockSlim slimLock = new ReaderWriterLockSlim();
        private readonly LongTermSortedBlocks longTerm = new LongTermSortedBlocks();

        public NodeLookupSlimLockWithLongTerm() 
        {
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
        }
        
        public SolverStatistics Statistics { get; }
        public string TypeDescriptor => $"SlimLockOverBlock[{LongTermBlock.SortedBlockSize:#,##0}]";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => null;

        public SolverNode? FindMatch(SolverNode find)
        {
            var ll =  longTerm.FindMatchFrozen(find);
            if (ll != null) return null;
            
            slimLock.EnterReadLock();
            try
            {
                return longTerm.FindMatchCurrent(find);
            }
            finally
            {
                slimLock.ExitReadLock();
            }
        }

        public void Add(SolverNode node)
        {
            slimLock.EnterWriteLock();
            try
            {
                longTerm.Add(node);
                Statistics.TotalNodes++;
            }
            finally
            {
                slimLock.ExitWriteLock();
            }
        }

        public void Add(IReadOnlyCollection<SolverNode> nodes)
        {
            slimLock.EnterWriteLock();
            try
            {
                longTerm.Add(nodes);
                Statistics.TotalNodes += nodes.Count;
            }
            finally
            {
                slimLock.ExitWriteLock();
            }
        }

        public IEnumerable<SolverNode> GetAll()
        {
            slimLock.EnterReadLock();
            try
            {
                foreach (var n in longTerm.GetAll())
                {
                    if (n != null) yield return n;
                }
            }
            finally
            {
                slimLock.ExitReadLock();
            }
           
        }
        


        class LongTermSortedBlocks
        {
            LongTermBlock current = new LongTermBlock();
            List<LongTermBlock> frozenBlocks = new List<LongTermBlock>();
            
            public void Add(SolverNode node)
            {
                current.Add(node);
                
                if (current.Count >= LongTermBlock.SortedBlockSize)
                {
                    frozenBlocks.Add(current);
                    current = new LongTermBlock();
                }
            }
            
            public void Add(IReadOnlyCollection<SolverNode> buffer)
            {
                current.Add(buffer);
                
                if (current.Count >= LongTermBlock.SortedBlockSize)
                {
                    frozenBlocks.Add(current);
                    current = new LongTermBlock();
                }
            }
            
            public SolverNode? FindMatchCurrent(SolverNode node) => current.FindMatch(node);

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
            public const int SortedBlockSize = 200_000;
            private List<SolverNode> block = new List<SolverNode>(SortedBlockSize);

            public int Count => block.Count;
            
            public void Add(IReadOnlyCollection<SolverNode> solverNodes)
            {
                block.AddRange(solverNodes);
                block.Sort();
            }
            
            public void Add(SolverNode node)
            {
                block.Add(node);        // SLOW!!! (but should never be used)
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

        public bool TrySample(out SolverNode? node)
        {
            throw new NotImplementedException();
        }
    }
}