using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SokoSolve.Core.Solver
{
    public class SolverQueue : ISolverQueue
    {
        private readonly Queue<SolverNode> inner;

        public SolverQueue()
        {
            inner = new Queue<SolverNode>();
            Statistics = new SolverStatistics
            {
                Name = GetType().Name
            };
        }

        public SolverStatistics Statistics { get; }

        public void Enqueue(SolverNode node)
        {
            Statistics.TotalNodes++;

            inner.Enqueue(node);
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            foreach (var node in nodes) Enqueue(node);
        }

        public SolverNode Dequeue()
        {
            if (inner.Count > 0)
            {
                Statistics.TotalNodes--;
                return inner.Dequeue();
            }

            return null;
        }

        public SolverNode[] Dequeue(int count)
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

    public class SolverQueueConcurrent : ISolverQueue
    {
        readonly ConcurrentQueue<SolverNode> queue = new ConcurrentQueue<SolverNode>();

        public bool TrySample(out SolverNode? node) => queue.TryPeek(out node);

        public SolverStatistics Statistics { get; } = new SolverStatistics();
        
        public void Enqueue(SolverNode node)
        {
            queue.Enqueue(node);
            Statistics.TotalNodes++;
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            foreach (var n in nodes)
            {
                Enqueue(n);
            }
        }

        public SolverNode Dequeue()
        {
            if (queue.TryDequeue(out var r))
            {
                Statistics.TotalNodes--;
                return r;
            }
            return null;
        }

        public SolverNode[] Dequeue(int count)
        {
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