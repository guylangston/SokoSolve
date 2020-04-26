using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SokoSolve.Core.Solver
{

    public class SolverPoolSlimRwLock : ISolverPoolChained
    {
        private readonly ISolverPool inner;
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        
        public SolverPoolSlimRwLock(ISolverPool inner)
        {
            this.inner = inner;
        }
        
        public ISolverPool InnerPool => inner;

        public SolverStatistics Statistics => inner.Statistics;
        public string TypeDescriptor => $"{GetType().Name}:sl";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) =>
            new[]
            {
                ("Cmd.Name", "sl"),
                ("Description", "ReaderWriterLockSlim wrap over inner pool")
            };

        public void Add(SolverNode node)
        {
            try
            {
                locker.EnterWriteLock();
              
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