using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SokoSolve.Core.Solver
{

    public class SolverNodeLookupThreadSafeWrapper : ISolverNodeLookup
    {
        private readonly ISolverNodeLookup inner;
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private SolverNode last;

        public SolverNodeLookupThreadSafeWrapper() : this(new SolverNodeLookupByBucket())
        {
        }

        public SolverNodeLookupThreadSafeWrapper(ISolverNodeLookup inner)
        {
            this.inner = inner;
        }

        public SolverStatistics Statistics => inner.Statistics;

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

        public IEnumerable<SolverNode> GetAll()
        {
            try
            {
                locker.EnterReadLock();
                return inner.GetAll();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public bool TrySample(out SolverNode node)
        {
            node = last;
            return last != null;
        }
    }

   
}