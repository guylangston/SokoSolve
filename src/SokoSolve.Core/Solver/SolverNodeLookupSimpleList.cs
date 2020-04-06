using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeLookupSimpleList : ISolverNodeLookup
    {
        private readonly List<SolverNode> items = new List<SolverNode>();
        
        public SolverNodeLookupSimpleList()
        {
            Statistics = new SolverStatistics
            {
                Name = GetType().Name
            };
        }

        private SolverNode?      last = null;
        public  SolverStatistics Statistics     { get; }
        public  string           TypeDescriptor => GetType().Name;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverCommandResult state) => throw new NotSupportedException();

        public bool TrySample(out SolverNode? node)
        {
            node = last;
            return true;
        }

        public void Add(SolverNode node)
        {
            Debug.Assert(node != null);
            last = node;
            items.Add(node);
            Statistics.TotalNodes = items.Count;
        }

        public void Add(IReadOnlyCollection<SolverNode> nodes)
        {
            Debug.Assert(nodes.All(x=>x != null));
            items.AddRange(nodes);
            last = items.Last();
            
            Statistics.TotalNodes = items.Count;
        }

        public SolverNode? FindMatch(SolverNode find) 
            => items.Find(x=>x.CompareTo(find) == 0);

        public IEnumerable<SolverNode> GetAll() => items;
    }
}