using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeLookupLongTerm : ISolverNodeLookupBatching
    {
        public   int                          MinBlockSize = 200_000;
        readonly ConcurrentBag<LongTermBlock> frozenBlocks = new ConcurrentBag<LongTermBlock>();
        public   SolverStatistics             Statistics { get; } = new SolverStatistics();
        
        public string TypeDescriptor => $"LongTermImmutable Blocks[{MinBlockSize}]";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverCommandResult state) => throw new NotSupportedException();
        
        public void Add(SolverNode n) => throw new NotSupportedException();

        public bool IsReadyToAdd(IReadOnlyCollection<SolverNode> buffer) => buffer.Count >= MinBlockSize;
        
        public void Add(IReadOnlyCollection<SolverNode> buffer)
        {
            Debug.Assert(ListHelper.IsSorted(buffer, SolverNode.ComparerInstance));
            
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
                var i =  block.BinarySearch(node, SolverNode.ComparerInstance);
                if (i < 0) return null;
                return block[i];
            }

            public IEnumerable<SolverNode> GetAll() => block;
        }
    }
}