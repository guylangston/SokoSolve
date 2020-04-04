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
    public class SolverNodeLookupLinkedListLongTerm : ISolverNodeLookup
    {
        private readonly LongTermSortedBlocks longTerm  = new LongTermSortedBlocks();

        public SolverNodeLookupLinkedListLongTerm()
        {
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
        }
        
        public SolverStatistics Statistics { get; }
        public string TypeDescriptor => $"SortedLinkedList, then LongTerm[{LongTermBlock.SortedBlockSize:#,##0}] **NOLOCK** NotStrictlySafe";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverCommandResult state) => throw new NotSupportedException();
        
        public void Add(SolverNode node)
        {
            longTerm.Add(node);
        }

        public void Add(IReadOnlyCollection<SolverNode> nodes)
        {
            longTerm.Add(nodes);
        }

        public SolverNode? FindMatch(SolverNode find)
        {
            var ll =  longTerm.FindMatchFrozen(find);
            if (ll != null) return null;
            
            return longTerm.FindMatchCurrent(find);
        }

        public IEnumerable<SolverNode> GetAll()
        {
            throw new NotImplementedException();
        }

        public bool TrySample(out SolverNode? node)
        {
            node = null;
            return false;
        }


        class LongTermSortedBlocks
        {
            LinkedList<SolverNode> current = new LinkedList<SolverNode>();
            ConcurrentBag<LongTermBlock> frozenBlocks = new ConcurrentBag<LongTermBlock>();
            
            public void Add(SolverNode n)
            {
                current.InsertSorted(n,(a, b) => SolverNode.ComparerInstance.Compare(a, b));
                
                if (current.Count >= LongTermBlock.SortedBlockSize)
                {
                    frozenBlocks.Add(new LongTermBlock(current.ToArray()));
                }
            }
            
            public void Add(IReadOnlyCollection<SolverNode> buffer)
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