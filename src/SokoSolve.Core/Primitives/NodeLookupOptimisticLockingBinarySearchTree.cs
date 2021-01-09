using System.Collections.Generic;
using System.Collections.Immutable;
using SokoSolve.Core.Solver;

namespace SokoSolve.Core.Primitives
{
    public abstract class BaseComponent : IExtendedFunctionalityDescriptor, IStatisticsProvider
    {
        protected BaseComponent()
        {
            TypeDescriptor = GetType().Name;
            Statistics = new SolverStatistics()
            {
                Name = TypeDescriptor,
                Type = TypeDescriptor,
            };
        }

        public string TypeDescriptor { get; protected set; }

        public virtual IEnumerable<(string name, string? text)> GetTypeDescriptorProps(SolverState state) => ImmutableArray<(string name, string? text)>.Empty;
        public SolverStatistics Statistics { get; }
    }
    
    
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