using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SokoSolve.Core.Solver
{

    public class SolverPoolSlimRwLock : ISolverPool
    {
        private readonly ISolverPool inner;
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private SolverNode last;

        
        public SolverPoolSlimRwLock(ISolverPool inner)
        {
            this.inner = inner;
        }

        public SolverStatistics Statistics => inner.Statistics;
        public string TypeDescriptor => GetType().Name;
        public  IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state) => throw new NotSupportedException();

        public void Add(SolverNode node)
        {
            try
            {
                locker.EnterWriteLock();
                last = node;
                inner.Add(node);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void Add(IReadOnlyCollection<SolverNode> nodes)
        {
            try
            {
                locker.EnterWriteLock();
                inner.Add(nodes);
                if (nodes.Any())
                {
                    last = nodes.Last();
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public SolverNode FindMatch(SolverNode find)
        {
            try
            {
                locker.EnterReadLock();
                return inner.FindMatch(find);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

      
    }

   
}