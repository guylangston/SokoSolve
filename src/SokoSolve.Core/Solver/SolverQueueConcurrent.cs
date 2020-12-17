using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SokoSolve.Core.Solver
{
    public class SolverQueueConcurrent : ISolverQueue
    {
        private readonly ConcurrentQueue<SolverNode> queue = new ConcurrentQueue<SolverNode>();
        
        public SolverStatistics Statistics { get; } = new SolverStatistics();
        public string TypeDescriptor => GetType().Name;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => null;

        public int Count => queue.Count;
        
        public bool TrySample(out SolverNode? node) => queue.TryPeek(out node);
        
        public void Enqueue(SolverNode node)
        {
            queue.Enqueue(node);
            Statistics.TotalNodes++;
            Statistics.CurrentNodes++;
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            foreach (var n in nodes)
            {
                Enqueue(n);
            }
        }

        public virtual SolverNode? Dequeue()
        {
            if (queue.TryDequeue(out var r))
            {
                Statistics.CurrentNodes--;
                return r;
            }
            return null;
        }

        public virtual IReadOnlyCollection<SolverNode>? Dequeue(int count)
        {
            var l = new List<SolverNode>();
            while (l.Count < count && queue.TryDequeue(out var r))
            {
                Statistics.CurrentNodes--;
                l.Add(r);
            }

            return l.ToArray();
        }
    }
}