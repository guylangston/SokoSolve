using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;
using System.Xml;

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


    public class LocalSolverQueue : SolverQueueConcurrent
    {
        private readonly List<LocalSolverQueue> siblings;

        public LocalSolverQueue(List<LocalSolverQueue> siblings)
        {
            siblings.Add(this);
            this.siblings = siblings;
        }

        public override SolverNode Dequeue()
        {
            if (base.queue.Count == 0)
            {
                TopUp();
            }
            if (queue.TryDequeue(out var r))
            {
                Statistics.TotalNodes--;
                return r;
            }
            return null;
        }

        private void TopUp()
        {
            foreach (var sib in siblings.OrderBy(x=>x.Count))
            {
                if (sib == this) continue;

                if (sib.queue.Count > 1 && sib.queue.TryDequeue(out var n))
                {
                    this.queue.Enqueue(n);
                    return;
                }
            }
            
            // Nothing; try again
            Thread.Sleep(10);
            
            foreach (var sib in siblings.OrderBy(x=>x.Count))
            {
                if (sib == this) continue;

                if (sib.queue.Count > 1 && sib.queue.TryDequeue(out var n))
                {
                    this.queue.Enqueue(n);
                    return;
                }
            }
        }

        public override SolverNode[] Dequeue(int count)
        {
            if (base.queue.Count == 0)
            {
                TopUp();
            }
            
            var l = new List<SolverNode>();
            while (l.Count < count && queue.TryDequeue(out var r))
            {
                Statistics.TotalNodes--;
                l.Add(r);
            }

            return l.ToArray();
        }
    }
}