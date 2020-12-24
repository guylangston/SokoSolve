using System.Collections.Generic;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Solver
{
    public class NodeLookupBinarySearchTree : INodeLookupChained
    {
        public NodeLookupBinarySearchTree(INodeLookupBatching longTerm)
        {
            this.longTerm = longTerm;
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
        }
        
        readonly BinarySearchTree<SolverNode> current = new BinarySearchTree<SolverNode>(SolverNode.ComparerInstanceFull);
        readonly INodeLookupBatching          longTerm;
        
        public SolverStatistics Statistics     { get; }
        public string           TypeDescriptor => $"BinarySearchTree:bst[{longTerm.MinBlockSize}] ==> {longTerm.TypeDescriptor}";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) =>
            new[]
            {
                ("Cmd.Name", "bst")
            };

        public INodeLookup InnerPool => longTerm;

        public void Add(SolverNode n)
        {
            Statistics.TotalNodes++;
            current.Add(n);
                
            if (longTerm.IsReadyToAdd(current))
            {
                longTerm.Add(current);
                current.Clear();
            }
        }
            
        public void Add(IReadOnlyCollection<SolverNode> buffer)
        {
            Statistics.TotalNodes += buffer.Count;
            current.AddRange(buffer);
            
            if (longTerm.IsReadyToAdd(current))
            {
                longTerm.Add(current);
                current.Clear();
            }
        }
        
        public SolverNode? FindMatch(SolverNode find)
        {
            var ll =  longTerm.FindMatch(find);
            if (ll != null) return null;
            
            return FindMatchCurrent(find);
        }
        
        SolverNode? FindMatchCurrent(SolverNode node) => current.FindOrDefault(node);



    }
}