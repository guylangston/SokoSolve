using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver
{
    public class SimpleSolverNodeLookup : ISolverNodeLookup
    {
        private readonly List<SolverNode> items = new List<SolverNode>();
        
        public SimpleSolverNodeLookup()
        {
            Statistics = new SolverStatistics
            {
                Name = GetType().Name
            };
        }

        private SolverNode?      last = null;
        public  SolverStatistics Statistics { get; }

        public bool TrySample(out SolverNode? node)
        {
            node = last;
            return true;
        }

        public void Add(SolverNode node)
        {
            last = node;
            items.Add(node);
        }

        public void Add(IEnumerable<SolverNode> nodes)
        {
            items.AddRange(nodes);
            last = items.Last();
        }

        public SolverNode FindMatch(SolverNode node) 
            => items.Find(x=>x.CompareTo(node) == 0);
    }
}