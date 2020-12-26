using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver
{
    public class SolverQueueConcurrent : SolverQueue
    {
        // TODO
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
        public string           TypeDescriptor => GetType().Name;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => null;
        
        
        
        public SolverNode? FindMatch(SolverNode find)
        {
            return inner.FirstOrDefault(x => x.Equals(find));
        }

        public int Count => inner.Count;
        
        public void Init(SolverState state) {}

        public void Enqueue(SolverNode node)
        {
            Statistics.TotalNodes++;

            inner.Enqueue(node);
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
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

        public bool TrySample(out SolverNode node)
        {
            node = inner.Peek();
            return true;
        }
    }

}