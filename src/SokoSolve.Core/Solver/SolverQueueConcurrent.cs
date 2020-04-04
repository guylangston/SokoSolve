using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SokoSolve.Core.Solver
{
    public class SolverQueueConcurrent : ISolverQueue
    {
        readonly ConcurrentQueue<SolverNode> queue = new ConcurrentQueue<SolverNode>();

        public bool TrySample(out SolverNode? node) => queue.TryPeek(out node);

        public SolverStatistics Statistics { get; } = new SolverStatistics();
        public string                                  GetTypeDescriptor                                 => null;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverCommandResult state) => throw new NotSupportedException();
        
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