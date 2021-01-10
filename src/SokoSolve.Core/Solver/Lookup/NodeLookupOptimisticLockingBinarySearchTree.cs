using System.Collections.Generic;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;

namespace SokoSolve.Core.Solver.Lookup
{


    public class NodeLookupOptimisticLockingBinarySearchTree : BaseComponent, INodeLookup
    {
        private readonly OptimisticLockingBinarySearchTree<SolverNode> inner 
            = new(SolverNode.ComparerInstanceFull, x => x.GetHashCode());

        public NodeLookupOptimisticLockingBinarySearchTree()
        {
            
        }

        public SolverNode? FindMatch(SolverNode find)
        {
            inner.TryFind(find, out var match);
            return match;
        }
        
        public bool IsThreadSafe => true;
        
        public void Add(SolverNode node)
        {
            Statistics.TotalNodes++;
            inner.TryAdd(node, out _);
        }
        public void Add(IReadOnlyCollection<SolverNode> nodes)
        {
            foreach (var n in nodes)
            {
                Statistics.TotalNodes++;
                inner.TryAdd(n, out _);
            }
        }
    }
}