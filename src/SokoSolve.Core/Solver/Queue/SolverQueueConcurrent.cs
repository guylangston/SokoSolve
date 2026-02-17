using System.Collections.Generic;

namespace SokoSolve.Core.Solver.Queue
{
    public class SolverQueueConcurrent : SolverQueue
    {

        public override bool IsThreadSafe => true;

        public override SolverNode? Dequeue()
        {
            lock (this)
            {
                return base.Dequeue();
            }
        }

        public override bool Dequeue(int count, List<SolverNode> dequeueInto)
        {
            lock (this)
            {
                return base.Dequeue(count, dequeueInto);
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
}
