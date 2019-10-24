using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeLookupThreadOptimised : SolverNodeLookup
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        protected override void Flush()
        {
            locker.EnterWriteLock();
            base.Flush();
            locker.ExitWriteLock();
        }
    }

    public class ThreadSafeSolverNodeLookupWrapper : ISolverNodeLookup
    {
        private readonly ISolverNodeLookup inner;
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public ThreadSafeSolverNodeLookupWrapper() : this(new SolverNodeLookup())
        {
        }


        public ThreadSafeSolverNodeLookupWrapper(ISolverNodeLookup inner)
        {
            this.inner = inner;
        }


        public SolverStatistics Statistics => inner.Statistics;

        private SolverNode last;

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