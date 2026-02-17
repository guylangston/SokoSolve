using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SokoSolve.Core.Solver.Lookup
{

    public class NodeLookupSlimRwLock : INodeLookupChained
    {
        private readonly INodeLookup inner;
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public NodeLookupSlimRwLock(INodeLookup inner)
        {
            this.inner = inner;
        }

        public INodeLookup InnerPool => inner;

        public bool IsThreadSafe => true;

        public SolverStatistics Statistics => inner.Statistics;
        public string TypeDescriptor => $"{GetType().Name}:lock";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) =>
            new[]
            {
                ("Cmd.Name", "lock"),
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

        public SolverNode? FindMatch(SolverNode find)
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
