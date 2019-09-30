using System.Collections.Generic;
using System.Threading;

namespace SokoSolve.Core.Solver
{
    public class ThreadSafeSolverNodeLookup : SolverNodeLookup
    {
        readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public ThreadSafeSolverNodeLookup() : base(new Queue<SolverNode>(), 6000)
        {
        }

        public override void Add(SolverNode node)
        {
            try
            {
                locker.EnterWriteLock();
                base.AddInnerBuffer(node);

            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public override void Add(IEnumerable<SolverNode> nodes)
        {
            try
            {
                locker.EnterWriteLock();
                foreach (var node in nodes)
                {
                    base.AddInnerBuffer(node);    
                }
                
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public override SolverNode FindMatch(SolverNode node)
        {
            try
            {
                locker.EnterReadLock();
                return base.FindMatch(node);

            }
            finally
            {
                locker.ExitReadLock();
            }
        }
    }
}