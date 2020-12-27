using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver.Lookup
{
    public class NodeLookupLongTerm : INodeLookupBatching
    {
        readonly ConcurrentBag<LongTermBlock> frozenBlocks = new ConcurrentBag<LongTermBlock>();

        public NodeLookupLongTerm()
        {
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
        }
        
        public bool IsThreadSafe => false;

        public int MinBlockSize { get; } = 200_000;
        public   SolverStatistics             Statistics { get;  }
        
        public string TypeDescriptor => $"LongTermImmutable:lt[{MinBlockSize}]";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) =>
            new[]
            {
                ("Cmd.Name", "lt")
            };

        
        public void Add(SolverNode n) => throw new NotSupportedException();

        public bool IsReadyToAdd(IReadOnlyCollection<SolverNode> buffer) => buffer.Count >= MinBlockSize;
        
        public void Add(IReadOnlyCollection<SolverNode> buffer)
        {
            Debug.Assert(ListHelper.IsSorted(buffer, SolverNode.ComparerInstanceFull));
            
            if (buffer.Count >= MinBlockSize)
            {
                frozenBlocks.Add(new LongTermBlock(buffer.ToImmutableArray()));
                Statistics.TotalNodes += buffer.Count;
            }
            else
            {
                throw new Exception("TooSmall");
            }
        }

     
        public SolverNode? FindMatch(SolverNode node)
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
         
            foreach (var block in frozenBlocks)
            {
                foreach (var n in block.GetAll())
                {
                    if (n != null) yield return n;
                }    
            }
        }

        public bool TrySample(out SolverNode? node)
        {
            node = null;
            return false;
        }
        
        class LongTermBlock
        {
            private readonly ImmutableArray<SolverNode> block;

            public LongTermBlock(ImmutableArray<SolverNode> block)
            {
                this.block = block;
            }
            
            public SolverNode? FindMatch(SolverNode node)
            {
                var i =  block.BinarySearch(node, SolverNode.ComparerInstanceFull);
                if (i < 0) return null;
                return block[i];
            }

            public IEnumerable<SolverNode> GetAll() => block;
        }
    }
}