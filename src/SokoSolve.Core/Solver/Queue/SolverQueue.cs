using System;
using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver.Queue
{

    public class SolverQueue : BaseComponent, ISolverQueue 
    {
        private readonly Queue<SolverNode> inner;

        public SolverQueue()
        {
            inner = new Queue<SolverNode>();
        }
        
                
        public virtual SolverNode? FindMatch(SolverNode find) => inner.FirstOrDefault(x => x != null && x.Equals(find));

        public int Count => inner.Count;

        public virtual bool IsThreadSafe => false;
        
        
        public void Init(SolverState state, SolverQueueMode mode) {}

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
        
        public virtual bool Dequeue(int count, List<SolverNode> dequeueInto)
        {
            var cc = 0;
            while (cc < count)
            {
                var d = Dequeue();
                if (d == null) return cc > 0;
                dequeueInto.Add(d);
                cc++;
            }
            return true;
        }
        

    }

}