using System;
using System.Collections.Generic;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
    public class SolverPoolSortedLinkedList : ISolverPoolChained
    {
        public SolverPoolSortedLinkedList(ISolverPoolBatching longTerm)
        {
            this.longTerm = longTerm;
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
        }
        
        readonly LinkedList<SolverNode> current = new LinkedList<SolverNode>();
        readonly ISolverPoolBatching longTerm;
        
        public SolverStatistics Statistics { get; }
        public string TypeDescriptor => $"SortedLinkedList:ll[{longTerm.MinBlockSize}] ==> {longTerm.TypeDescriptor}";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) =>
            new[]
            {
                ("Cmd.Name", "ll")
            };

        public ISolverPool InnerPool => longTerm;

        public void Add(SolverNode n)
        {
            Statistics.TotalNodes++;
            current.InsertSorted(n,(a, b) => SolverNode.ComparerInstanceFull.Compare(a, b));
                
            if (longTerm.IsReadyToAdd(current))
            {
                longTerm.Add(current);
                current.Clear();
            }
        }
            
        public void Add(IReadOnlyCollection<SolverNode> buffer)
        {
            Statistics.TotalNodes+=buffer.Count;
            foreach (var n in buffer)
            {
                current.InsertSorted(n,(a, b) => SolverNode.ComparerInstanceFull.Compare(a, b));
            }
                
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
        
        SolverNode? FindMatchCurrent(SolverNode node) 
            => current.FindInSorted(node, (a, b) => SolverNode.ComparerInstanceFull.Compare(a, b));



    }
}