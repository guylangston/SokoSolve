using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Sokoban.Core.Common;

namespace Sokoban.Core.Solver
{
    public class SolverQueue : ISolverQueue
    {
        private readonly Queue<SolverNode> inner;
      
        public SolverQueue()
        {
            inner = new Queue<SolverNode>();
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
        }

        public SolverStatistics Statistics { get; private set; }

        public void Enqueue(SolverNode node)
        {
            Statistics.TotalNodes++;
            
            inner.Enqueue(node);
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            foreach (var node in nodes)
            {
                Enqueue(node);
            }
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
            int cc = 0;
            while(cc < count)
            {
                var d = Dequeue();
                if (d == null) break;
                res.Add(d);
                cc++;
            }
            return res.ToArray();
        }
    }
}