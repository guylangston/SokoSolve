using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeLookupLinked : ISolverNodeLookup
    {
        public SolverNodeLookupLinked(ISolverNodeLookupBatching longTerm)
        {
            this.longTerm = longTerm;
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
        }
        
        readonly LinkedList<SolverNode> current = new LinkedList<SolverNode>();
        readonly ISolverNodeLookupBatching longTerm;
        
        public SolverStatistics Statistics { get; }
        public string TypeDescriptor => $"SortedLinkedList ==> {longTerm.TypeDescriptor}";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverCommandResult state) => throw new NotSupportedException();
        
        
        public void Add(SolverNode n)
        {
            current.InsertSorted(n,(a, b) => SolverNode.ComparerInstance.Compare(a, b));
                
            if (longTerm.IsReadyToAdd(current))
            {
                longTerm.Add(current);
                current.Clear();
            }
        }
            
        public void Add(IReadOnlyCollection<SolverNode> buffer)
        {
            foreach (var n in buffer)
            {
                current.InsertSorted(n,(a, b) => SolverNode.ComparerInstance.Compare(a, b));
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
            => current.FindInSorted(node, (a, b) => SolverNode.ComparerInstance.Compare(a, b));

        public IEnumerable<SolverNode> GetAll()
        {
            foreach (var n in current)
            {
                 yield return n;
            }

            foreach (var n in longTerm.GetAll())
            {
                yield return n;    
            }
                
        }

        public bool TrySample(out SolverNode? node)
        {
            node = null;
            return false;
        }
        
        

    }
}