using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SokoSolve.Core.Solver.Queue
{
    /// <summary>
    /// Features/Goals:
    /// - ConstantSpeed/Fast Enqueue/Dequeue
    /// - ConstantSpeed/Fast Lookup
    /// - ThreadSafe
    /// </summary>
    public class SolverQueueSortedWithDeDup : ISolverQueue
    {
        private Queue<SolverNode> queue = new Queue<SolverNode>();
        
        public SolverQueueSortedWithDeDup()
        {
            Statistics = new SolverStatistics();
        }

        public bool IsThreadSafe => true;

        public void Init(SolverState state) {}

        public SolverNode? FindMatch(SolverNode find)
        {
            Debug.Assert(find != null);
            lock (this)
            {
                return FindMatchInner(find);    
            }
            
        }
        
        private SolverNode? FindMatchInner(SolverNode node)
        {
            return queue.FirstOrDefault(x=>x.Equals(node));
        }
        
        private void AddForLookup(SolverNode node)
        {
            // nothing
        }
        
        private void RemoveForLookup(SolverNode node)
        {
            // nothing
        }


        public void Enqueue(SolverNode node)
        {
            Debug.Assert(node != null);
            lock (this)
            {
                var alreadyExists = FindMatchInner(node);
                if (alreadyExists == null)
                {
                    queue.Enqueue(node);
                    AddForLookup(node);
                }
            }
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            lock (this)
            {
                foreach (var node in nodes)
                {
                    var alreadyExists = FindMatchInner(node);
                    if (alreadyExists == null)
                    {
                        queue.Enqueue(node);
                        AddForLookup(node);
                    }
                }
            }
        }
        
        
        public SolverNode? Dequeue()
        {
            lock (this)
            {
                if(queue.TryDequeue(out var outNode))
                {
                    RemoveForLookup(outNode);
                    return outNode;
                }
                return null;

            }
        }
        public bool Dequeue(int count, List<SolverNode> dequeueInto)
        {
            lock (this)
            {
                for (int cc = 0; cc < count; cc++)
                {
                    if(queue.TryDequeue(out var outNode))
                    {
                        RemoveForLookup(outNode);
                        dequeueInto.Add(outNode);
                    }
                    else
                    {
                        return cc > 0;
                    }
                }
                return true;
            }
        }

        public SolverStatistics Statistics     { get; }
        public string           TypeDescriptor => GetType().Name;
        public IEnumerable<(string name, string? text)> GetTypeDescriptorProps(SolverState state) => null;
    }
}