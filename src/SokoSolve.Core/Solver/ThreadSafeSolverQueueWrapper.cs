using System;
using System.Collections.Generic;

namespace SokoSolve.Core.Solver
{
    public class ThreadSafeSolverQueueWrapper : ISolverQueue
    {
        private readonly ISolverQueue inner;

        public ThreadSafeSolverQueueWrapper(ISolverQueue inner)
        {
            this.inner = inner;
        }

        public SolverStatistics Statistics => inner.Statistics;
        public string                                  GetTypeDescriptor                                 => null;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverCommandResult state) => throw new NotSupportedException();

        public void Enqueue(SolverNode node)
        {
            lock (this)
            {
                inner.Enqueue(node);
            }
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            lock (this)
            {
                inner.Enqueue(nodes);
            }
        }

        public SolverNode Dequeue()
        {
            lock (this)
            {
                return inner.Dequeue();
            }
        }

        public SolverNode[] Dequeue(int count)
        {
            lock (this)
            {
                return inner.Dequeue(count);
            }
        }
        
        public bool TrySample(out SolverNode node)
        {
            node = default;
            return false; // not thread sage
        }
    }
}