using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver.Lookup
{
    public class NodeLookupByDepth : INodeLookup
    {
        private List<INodeLookup> byDepth = new List<INodeLookup>(100);

        public bool IsThreadSafe => false;

        public SolverStatistics Statistics     { get; } = new SolverStatistics();
        public string           TypeDescriptor => null;
        public IEnumerable<(string name, string? text)> GetTypeDescriptorProps(SolverState state) => null;

        protected virtual INodeLookup CreateInnerNodeLookup() => new NodeLookupSimpleList();

        public NodeLookupByDepth()
        {
        }

        public NodeLookupByDepth(SolverNode root)
        {
            foreach (var kid in root.Recurse())
            {
                Add(kid);
            }
        }

        public int Depth => byDepth.Count;
        public INodeLookup this[int depth] => byDepth[depth];

        public IReadOnlyList<INodeLookup> GetLayers() => byDepth.ToArray();

        public SolverNode? FindMatch(SolverNode find)
        {
            var d = find.GetDepth();
            return byDepth[d]?.FindMatch(find);
        }

        public void Add(SolverNode node)
        {
            var d = node.GetDepth();
            while (byDepth.Count() <= d)
            {
                byDepth.Add(CreateInnerNodeLookup());
            }
            byDepth[d].Add(node);
        }

        public void Add(IReadOnlyCollection<SolverNode> nodes)
        {
            foreach (var node in nodes)
            {
                Add(node);
            }
        }
    }
}
