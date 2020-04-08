using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SokoSolve.Core.Solver
{
    public class SolverPoolByBucket : ISolverPool
    {
        private const    int          BufferMax = 5_000;
        private readonly List<Bucket> buckets;
        private readonly int          maxBucketSize;
        private          SolverNode[] buffer;
        private volatile int          bufferNext;
        private          int          largest;
        private          int          smallest;
        
        public SolverStatistics Statistics { get; }

        public string TypeDescriptor => $"{GetType().Name}:bucket";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state) =>
            new[]
            {
                ("Cmd.Name", "bucket"),
                ("ThreadSafe","False")
            };

        public SolverPoolByBucket(int maxBucketSize = 100_000)
        {
            Statistics = new SolverStatistics
            {
                Name = GetType().Name
            };
            this.maxBucketSize = maxBucketSize;
            this.buffer        = new SolverNode[BufferMax];
            this.buckets       = new List<Bucket>();
        }
        
        public virtual void Add(SolverNode node) => AddInnerBuffer(node);
        public virtual void Add(IReadOnlyCollection<SolverNode> nodes)
        {
            foreach (var node in nodes) AddInnerBuffer(node);
        }
        
        public virtual SolverNode FindMatch(SolverNode find)
        {
            // first buffer
            if (buffer.Length > 0)
            {
                var h = find.GetHashCode();
                for (var i = 0; i < buffer.Length; i++)
                {
                    var b = buffer[i];
                    if (b == null) continue;

                    if (h == b.GetHashCode() && find.Equals(b)) return b;
                }
            }

            // then main pool
            var hash = find.GetHashCode();
            foreach (var bucket in buckets)
            {
                if (bucket.smallest <= hash && hash <= bucket.largest)
                {
                    var idx = bucket.BinarySearch(find);
                    if (idx >= 0) return bucket[idx];
                }
            }
            
            return null;
        }

        public IEnumerable<SolverNode> GetAll()
        {
            throw new NotImplementedException();
        }

        protected void AddInnerBuffer(SolverNode node)
        {
            Statistics.TotalNodes++;
            var nn = Interlocked.Increment(ref bufferNext);
            
            buffer[nn] = node;
            if (nn == BufferMax - 1)
            {
                Flush();
            }
        }

        protected virtual void Flush()
        {
            var buf = buffer;
            buffer = new SolverNode[BufferMax];
            bufferNext = 0;

            for (var cc = 0; cc < BufferMax; cc++)
            {
                if (buf[cc] == null) continue;

                AddInnerBuckets(buf[cc]);
            }
            
            // Update Stats
            Statistics.TotalDead = buckets.Sum(x => x.Count(y => y.Status == SolverNodeStatus.Dead));
        }

        protected void AddInnerBuckets(SolverNode node)
        {
            var hash = node.GetHashCode();
            if (buckets.Count == 0)
            {
                var b = new Bucket(maxBucketSize);
                smallest = largest = b.smallest = b.largest = hash;
                b.Add(node);
                buckets.Add(b);
            }
            else
            {
                if (hash > largest)
                {
                    var last = buckets.Last();

                    last.Add(node);
                    largest = last.largest = hash;
                    if (last.Count > maxBucketSize) Split(last);
                }
                else if (hash < smallest)
                {
                    var first = buckets.First();

                    first.Insert(0, node);
                    smallest = first.smallest = hash;
                    if (first.Count > maxBucketSize) Split(first);
                }
                else
                {
                    var b = FindBucket(hash);
                    if (b == null) throw new InvalidOperationException();

                    b.AddSorted(node, hash);
                    // Add
                    if (b.Count > maxBucketSize) Split(b);
                }
            }
        }

        private Bucket FindBucket(int hash)
        {
            // Assumes ordered 
            return buckets.FirstOrDefault(c => hash <= c.largest);
        }


        private void Split(Bucket bucket)
        {
            var split = bucket.Count / 2;

            var bl = new Bucket(bucket.Take(split));
            var br = new Bucket(bucket.Skip(split));
            var idx = buckets.IndexOf(bucket);
            buckets.Insert(idx, bl);
            buckets[idx + 1] = br;

            bucket.Clear();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var cc = 0;
            foreach (var bucket in buckets)
            {
                sb.AppendFormat("#{0,-3}(Size:{3,3}) {1} to {2}", cc++, bucket.smallest, bucket.largest, bucket.Count);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public bool TrySample(out SolverNode node)
        {
            node = default;
            return false; // not thread sage
        }
 
        private class Bucket : List<SolverNode>
        {
            public int largest;
            public int smallest;

            public Bucket()
            {
            }

            public Bucket(int capacity)
                : base(capacity)
            {
            }

            public Bucket(IEnumerable<SolverNode> collection)
                : base(collection)
            {
                smallest = collection.Min(n => n.GetHashCode());
                largest = collection.Max(n => n.GetHashCode());
            }

            public void AddSorted(SolverNode node, int hash)
            {
                var cc = 0;
                while (cc < Count && this[cc].GetHashCode() < hash) cc++;
                Insert(cc, node);

                if (cc == 0)
                    smallest = hash;
                else if (cc == Count - 1) largest = hash;
            }
        }

        
    }
}