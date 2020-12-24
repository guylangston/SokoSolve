using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver.Lookup
{
    public class NodeLookupSimpleList : INodeLookup
    {
        private readonly List<SolverNode> items = new List<SolverNode>();
        
        public NodeLookupSimpleList()
        {
            Statistics = new SolverStatistics
            {
                Name = GetType().Name
            };
        }

        private SolverNode?      last = null;
        public  SolverStatistics Statistics     { get; }
        public string TypeDescriptor => $"{GetType().Name}:ListT";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) =>
            new[]
            {
                ("Cmd.Name", "ListT"),
                ("Description", items.GetType().Name)
            };

        public List<SolverNode> GetInnerList() => items;

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