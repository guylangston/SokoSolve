using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Transactions;
using System.Xml;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
    public class SolverQueue : ISolverQueue
    {
        protected readonly Queue<SolverNode> inner;

        public SolverQueue()
        {
            inner = new Queue<SolverNode>();
            Statistics = new SolverStatistics
            {
                Name = GetType().Name
            };
        }

        public SolverStatistics Statistics { get; }
        public string                                  TypeDescriptor                                 => GetType().Name;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => null;

        public int Count => inner.Count;

        public void Enqueue(SolverNode node)
        {
            Statistics.TotalNodes++;

            inner.Enqueue(node);
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            foreach (var node in nodes) Enqueue(node);
        }

        public virtual SolverNode Dequeue()
        {
            if (inner.Count > 0)
            {
                Statistics.TotalNodes--;
                return inner.Dequeue();
            }

            return null;
        }

        public virtual SolverNode[] Dequeue(int count)
        {
            var res = new List<SolverNode>(count);
            var cc  = 0;
            while (cc < count)
            {
                var d = Dequeue();
                if (d == null) break;
                res.Add(d);
                cc++;
            }

            return res.ToArray();
        }

        public bool TrySample(out SolverNode node)
        {
            node = inner.Peek();
            return true;
        }
    }


    // Experimental: Seems way to slow for the resulting memory savings
    public class ReuseTreeSolverQueue : ISolverQueue
    {
        private SolverNode root;

        public ReuseTreeSolverQueue(SolverNode root)
        {
            this.root = root;
        }

        public SolverNode Root
        {
            get => root;
            set => root = value ?? throw new ArgumentNullException(nameof(value));
        }

        public SolverStatistics Statistics { get; } = new SolverStatistics();

        public string TypeDescriptor => "Reuse the tree";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => ImmutableArray<(string name, string text)>.Empty;

        public void Enqueue(SolverNode node){ }

        public void Enqueue(IEnumerable<SolverNode> nodes) { }

        public SolverNode? Dequeue()
        {
            SolverNode uneval = null;
            while (uneval == null || uneval.Status != SolverNodeStatus.UnEval)
            {
                uneval = root.RecursiveAll().FirstOrDefault(x => x.Status == SolverNodeStatus.UnEval);
            }

            return uneval;
        }

        public SolverNode[]? Dequeue(int count)
        {
            var res = new SolverNode[count];

            for (int i = 0; i < count; i++)
            {
                res[i] = Dequeue();
            }

            return res;
        }
    }
    
}