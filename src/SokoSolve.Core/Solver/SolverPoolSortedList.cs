using System;
using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver
{
    public class SolverPoolSortedList : ISolverPoolChained
    {
        public SolverPoolSortedList(ISolverPoolBatching longTerm)
        {
            this.longTerm = longTerm;
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
        }
        
        readonly List<SolverNode> current = new List<SolverNode>();
        readonly ISolverPoolBatching longTerm;
        
        public SolverStatistics Statistics     { get; }
        public string           TypeDescriptor => $"SortedList:sl[{longTerm.MinBlockSize}] ==> {longTerm.TypeDescriptor}";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state) =>
            new[]
            {
                ("Cmd.Name", "sl"),
                ("ThreadSafe","False")
            };

        public ISolverPool InnerPool => longTerm;

        public void Add(SolverNode n)
        {
            Statistics.TotalNodes++;
            InsertSorted(current, n);
                
            if (longTerm.IsReadyToAdd(current))
            {
                longTerm.Add(current);
                current.Clear();
            }
        }
            
        public void Add(IReadOnlyCollection<SolverNode> buffer)
        {
            Statistics.TotalNodes += buffer.Count;
            InsertSorted(current, buffer);
                
            if (longTerm.IsReadyToAdd(current))
            {
                longTerm.Add(current);
                current.Clear();
            }
        }
        
        // TODO: Add unit test
        private static void InsertSorted(List<SolverNode> list, SolverNode node)
        {
            var cc = 0;
            while (cc < list.Count && list[cc].GetHashCode() < node.GetHashCode()) 
                cc++;
            list.Insert(cc, node);
        }

        // TODO: Add unit test
        private static void InsertSorted(List<SolverNode> list, IReadOnlyCollection<SolverNode> buffer)
        {
            var cc = 0;
            foreach (var node in buffer.OrderBy(x=>x.GetHashCode()))
            {
                while (cc < list.Count && list[cc].GetHashCode() < node.GetHashCode()) 
                    cc++;
                list.Insert(cc, node);
            }
        }

        public SolverNode? FindMatch(SolverNode find)
        {
            var ll =  longTerm.FindMatch(find);
            if (ll != null) return null;
            
            return FindMatchCurrent(find);
        }
        
        SolverNode? FindMatchCurrent(SolverNode node)
        {
            var i = current.BinarySearch(node); 
            return i < 0 ? null : current[i];
        }
    }
}