using System.Collections.Generic;

namespace SokoSolve.Core.Solver.Queue
{
    public class SolverQueueLocalSlave : ISolverQueue
    {
        private readonly ISolverQueue master;
        
        public SolverQueueLocalSlave(ISolverQueue master)
        {
            this.master = master;
            Statistics  = new SolverStatistics();
        }
        
        public SolverNode? FindMatch(SolverNode find) => master.FindMatch(find);
        
        public SolverStatistics Statistics { get; }


        public bool IsThreadSafe => master.IsThreadSafe;
        
        public void Init(SolverState state)
        {
            
        }

        public void Enqueue(SolverNode node) => master.Enqueue(node);
        public void Enqueue(IEnumerable<SolverNode> nodes) => master.Enqueue(nodes);

        public SolverNode? Dequeue() => master.Dequeue();
        public bool Dequeue(int count, List<SolverNode> dequeueInto) => master.Dequeue(count, dequeueInto);
        
        public string TypeDescriptor => GetType().Name;
        public IEnumerable<(string name, string? text)> GetTypeDescriptorProps(SolverState state) => null;

    }
}