using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace SokoSolve.Client.Web.Logic
{
    public abstract class ServerSideStateLease
    {
        protected ServerSideStateLease(int leaseId, object? state, object? context, DateTime created, DateTime expires)
        {
            if (leaseId < 1) throw new ArgumentException(nameof(leaseId));
            LeaseId      = leaseId;
            State   = state;
            Context = context;
            Created = created;
            Expires = expires;
        }

        public int      LeaseId      { get; }
        public object?  State   { get; set; }
        public object?  Context { get; set; }
        public DateTime Created { get; }
        public DateTime Expires { get; }
    }

    public class ServerSideStateLease<T> : ServerSideStateLease
    {
        public ServerSideStateLease(int leaseId, T? state, object? context, DateTime created, DateTime expires) : base(leaseId, state, context, created, expires)
        {
        }

        public new T State
        {
            get => (T)base.State;
            set => base.State = value;
        }
    }

    public class ServerSideStateComponent
    {
        private volatile int nextId = 1000;
        private readonly ConcurrentDictionary<int, ServerSideStateLease> leases = new ConcurrentDictionary<int, ServerSideStateLease>();

        public TimeSpan DefaultLease { get; set; }

        public ServerSideStateComponent()
        {
            DefaultLease = TimeSpan.FromHours(1);
        }

        public ServerSideStateLease<T> CreateLease<T>(T state, object ctx = null)
        {
            var now = DateTime.Now;
            var x   = new ServerSideStateLease<T>(Interlocked.Increment(ref nextId), state, ctx, now, now + DefaultLease);
            leases[x.LeaseId] = x;
            return x;
        }


        public ServerSideStateLease<T> GetLease<T>(int id)
        {
            if (leases.TryGetValue(id, out var state))
            {
                if (state is ServerSideStateLease<T> upType) return upType;

                throw new Exception("State Found, but of incorrect Type");
            }

            throw new Exception("State Not Found Or Expired");
        }


        public bool TryGetLease<T>(int id,  [MaybeNullWhen(false)]  out  ServerSideStateLease<T> lease)
        {
            if (leases.TryGetValue(id, out var state))
            {
                if (state is ServerSideStateLease<T> upType)
                {
                    lease = upType;
                    return true;
                }
            }

            lease = default;
            return false;
        }
        
        
        public T GetLeaseData<T>(int id) => GetLease<T>(id).State;
    }

}