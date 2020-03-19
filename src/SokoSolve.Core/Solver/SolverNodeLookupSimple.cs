using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeLookupSimple : ISolverNodeLookup
    {
        private readonly List<SolverNode> items = new List<SolverNode>();
        
        public SolverNodeLookupSimple()
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

            Statistics.TotalNodes = items.Count;
        }

        public void Add(IEnumerable<SolverNode> nodes)
        {
            items.AddRange(nodes);
            last = items.Last();
            
            Statistics.TotalNodes = items.Count;
        }

        public SolverNode FindMatch(SolverNode node) 
            => items.Find(x=>x.CompareTo(node) == 0);
    }
}