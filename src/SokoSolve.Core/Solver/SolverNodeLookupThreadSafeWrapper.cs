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

        public void Add(IEnumerable<SolverNode> nodes)
        {
            try
            {
                locker.EnterWriteLock();
                inner.Add(nodes);
                if (nodes is IReadOnlyCollection<SolverNode> col && col.Any())
                {
                    last = col.Last();
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public SolverNode FindMatch(SolverNode node)
        {
            try
            {
                locker.EnterReadLock();
                return inner.FindMatch(node);
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