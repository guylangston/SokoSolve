using System;
using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver.Queue
{
    public class SolverQueueConcurrent : SolverQueue
    {
        public override SolverNode? Dequeue()
        {
            lock (this)
            {
                return base.Dequeue();    
            }
        }

        public override IReadOnlyCollection<SolverNode>? Dequeue(int count)
        {
            lock (this)
            {
                return base.Dequeue(count);    
            }
            
        }

        public override void Enqueue(SolverNode node)
        {
            lock (this)
            {
                base.Enqueue(node);    
            }
            
        }

        public override void Enqueue(IEnumerable<SolverNode> nodes)
        {
            lock (this)
            {
                base.Enqueue(nodes);    
            }
            
        }

        public override SolverNode? FindMatch(SolverNode find)
        {
            lock (this)
            {
                return base.FindMatch(find);    
            }
        }
    }
    
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

        public SolverStatistics Statistics     { get; }
                
        public virtual SolverNode? FindMatch(SolverNode find) => inner.FirstOrDefault(x => x != null && x.Equals(find));

        public int Count => inner.Count;
        
        public void Init(SolverState state) {}

        public virtual void Enqueue(SolverNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            Statistics.TotalNodes++;
            inner.Enqueue(node);
        }

        public virtual void Enqueue(IEnumerable<SolverNode> nodes)
        {
            foreach (var node in nodes) Enqueue(node);
        }

        public virtual SolverNode? Dequeue()
        {
            if (inner.Count > 0)
            {
                Statistics.TotalNodes--;
                return inner.Dequeue();
            }

            return null;
        }

        public virtual IReadOnlyCollection<SolverNode>? Dequeue(int count)
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

            return res;
        }

      
        public string TypeDescriptor => GetType().Name;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => null;

    }

}