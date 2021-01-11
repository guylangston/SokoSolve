using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SokoSolve.Core.Solver.Queue
{
    public class SortedSolverNodeList
    {
        private SortedSet<SolverNode> sorted = new SortedSet<SolverNode>(SolverNode.ComparerInstanceFull);  // Should be a Red/Black tree or similar

        public SortedSolverNodeList()
        {
     
        }
        
        public SolverNode? FindMatch(SolverNode find)
        {
            if (sorted.TryGetValue(find, out var match))
            {
                return match;
            }
            return null;
        }
        
        public void Add(SolverNode node)
        {
            sorted.Add(node);
        }
        
        // Remove by reference? Or all that are equal?
        public void Remove(SolverNode node)
        {
            sorted.Remove(node);            
        }
    }


    /// <summary>
    /// Features/Goals:
    /// - ConstantSpeed/Fast Enqueue/Dequeue
    /// - ConstantSpeed/Fast Lookup
    /// - ThreadSafe
    /// </summary>
    public class SolverQueueSortedWithDeDup : ISolverQueue
    {
        private Queue<SolverNode> queue = new Queue<SolverNode>();
        private SortedSolverNodeList inner = new SortedSolverNodeList();
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        
        public SolverQueueSortedWithDeDup()
        {
            Statistics = new SolverStatistics();
        }

        public bool            IsThreadSafe => true;
        public SolverQueueMode Mode         { get; set; }

        public void Init(SolverState state, SolverQueueMode mode)
        {
            this.Mode = mode;
        }

        public SolverNode? FindMatch(SolverNode find)
        {
            Debug.Assert(find != null);
            locker.EnterReadLock();
            var res = FindMatchInner(find);
            locker.ExitReadLock();
            return res;            
        }
        
        private SolverNode? FindMatchInner(SolverNode node)
        {
            if (Mode == SolverQueueMode.QueueOnly) return null;

            return inner.FindMatch(node);
        }
        private void AddForLookup(SolverNode node)
        {
            if (Mode == SolverQueueMode.QueueOnly) return;
            inner.Add(node);
        }
        private void RemoveForLookup(SolverNode node)
        {
            if (Mode == SolverQueueMode.QueueOnly) return;
            inner.Remove(node);
        }

        public void Enqueue(SolverNode node)
        {
            Debug.Assert(node != null);
            
            // Optimistic: A FindMatch has already been done (don't do it again)

            locker.EnterWriteLock();
            try
            {
                Statistics.TotalNodes++;
                Statistics.CurrentNodes++;
                queue.Enqueue(node);
                AddForLookup(node);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            locker.EnterWriteLock();

            try
            {
                foreach (var node in nodes)
                {
                    Statistics.TotalNodes++;
                    Statistics.CurrentNodes++;
                    queue.Enqueue(node);
                    AddForLookup(node);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }
        
        
        public SolverNode? Dequeue()
        {
            locker.EnterWriteLock();
            try
            {
                if (queue.TryDequeue(out var outNode))
                {
                    Statistics.CurrentNodes--;
                    RemoveForLookup(outNode);
                    return outNode;
                }
                return null;
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool Dequeue(int count, List<SolverNode> dequeueInto)
        {
            locker.EnterWriteLock();

            try
            {
                for (int cc = 0; cc < count; cc++)
                {
                    if (queue.TryDequeue(out var outNode))
                    {
                        Statistics.CurrentNodes--;
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
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public SolverStatistics Statistics     { get; }
        public string           TypeDescriptor => GetType().Name;
        public IEnumerable<(string name, string? text)> GetTypeDescriptorProps(SolverState state) => null;
    }
}