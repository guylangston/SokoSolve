using System.Collections.Generic;

namespace SokoSolve.Core.Solver.Queue
{
    public class SolverQueueLocalSlave : BaseComponent, ISolverQueue
    {
        private readonly ISolverQueue master;
        
        public SolverQueueLocalSlave(ISolverQueue master)
        {
            this.master = master;
        }
        
        public SolverNode? FindMatch(SolverNode find) => master.FindMatch(find);
        
        public bool IsThreadSafe => master.IsThreadSafe;
        
        public void Init(SolverQueueMode mode)
        {
            
        }

        public void Enqueue(SolverNode node) => master.Enqueue(node);
        public void Enqueue(IEnumerable<SolverNode> nodes) => master.Enqueue(nodes);

        public SolverNode? Dequeue() => master.Dequeue();
        public bool Dequeue(int count, List<SolverNode> dequeueInto) => master.Dequeue(count, dequeueInto);
        
        

    }
}