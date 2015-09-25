using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Sokoban.Core.Solver
{
    public class SolverNodeLookup : ISolverNodeLookup
    {
        private readonly int maxBucketSize;
        private readonly List<Bucket> buckets;
        private SolverNode[] buffer;
        private const int BufferMax = 1500;
        private int bufferNext = 0;
        private int smallest;
        private int largest;

        public SolverNodeLookup(int maxBucketSize = 10000) : this(new Queue<SolverNode>(), maxBucketSize)
        {
            
        }

        public SolverNodeLookup(Queue<SolverNode> buffer, int maxBucketSize)
        {
            Statistics = new SolverStatistics()
            {
                Name = GetType().Name
            };
            this.maxBucketSize = maxBucketSize;
            this.buffer = new SolverNode[BufferMax];
            buckets = new List<Bucket>();
        }

        public SolverStatistics Statistics { get; private set; }

        public TextWriter Report { get; set; }

        public virtual void Add(SolverNode node)
        {
            AddInnerBuffer(node);
        }

        protected void AddInnerBuffer(SolverNode node)
        {
            Statistics.TotalNodes++;
            if (bufferNext >= BufferMax)
            {
                Flush();
            }

            if (buffer[bufferNext] != null)
            {
                buffer[bufferNext++] = node;
                if (bufferNext == BufferMax) bufferNext = 0;
            }
            else
            {
                for (int cc = 0; cc < BufferMax; cc++)
                {
                    if (buffer[cc] == null)
                    {
                        buffer[cc] = node;
                        bufferNext = cc + 1;
                        return;
                    }
                }
                // Cannot allocate: so we flush bufer
                Flush();
            }
        }

        protected virtual void Flush()
        {
            var buf = buffer;
            buffer = new SolverNode[BufferMax];
            bufferNext = 0;

            for (int cc = 0; cc < BufferMax; cc++)
            {
                if (buf[cc] == null) continue;

                AddInnerBuckets(buf[cc]);
            }
        }

        protected  void AddInnerBuckets(SolverNode node)
        {
            var hash = node.Hash;
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
                    if (last.Count > maxBucketSize)
                    {
                        Split(last);
                    }
                    
                    
                } else if (hash < smallest)
                {
                    var first = buckets.First();

                    first.Insert(0, node); 
                    smallest = first.smallest = hash;
                    if (first.Count > maxBucketSize)
                    {
                        Split(first);
                    }
                }
                else
                {
                    var b = FindBucket(hash);
                    if (b == null)
                    {
                        throw new InvalidOperationException();
                    }
                    else
                    {
                        b.AddSorted(node, hash);
                        // Add
                        if (b.Count > maxBucketSize)
                        {
                            Split(b);
                        }
                    }
                }
            }
        }

        
        public virtual void Add(IEnumerable<SolverNode> nodes)
        {
            foreach (var node in nodes)
            {
                AddInnerBuffer(node);
            }
        }


        public virtual SolverNode FindMatch(SolverNode node)
        {
           
            // first buffer
            if (buffer.Length > 0)
            {
                var h = node.Hash;
                for (int i = 0; i < buffer.Length; i++)
                {
                    var b = buffer[i];
                    if (b == null) continue;

                    if (h == b.Hash && node.Equals(b)) return b;
                }
            }

            // then main pool
            var hash = node.Hash;
            foreach (var bucket in buckets)
            {
                if (bucket.smallest <= hash && hash <= bucket.largest)
                {
                    var idx = bucket.BinarySearch(node);
                    if (idx >= 0)
                    {
                        return bucket[idx];
                    }
                }
            }

            return null;
        }

        private class Bucket : List<SolverNode>
        {
            public int smallest;
            public int largest;


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
                smallest = collection.Min(n => n.Hash);
                largest = collection.Max(n => n.Hash);
            }

            public void AddSorted(SolverNode node, int hash)
            {
                var cc = 0;
                while (cc < Count && this[cc].Hash < hash)
                {
                    cc++;
                }
                Insert(cc, node);

                if (cc == 0)
                {
                    smallest = hash;
                }
                else if (cc == Count - 1)
                {
                    largest = hash;
                }
            }
        }

        private Bucket FindBucket(int hash)
        {
            // Assumes ordered 
            return buckets.FirstOrDefault(c => hash  <= c.largest);
        }

        
        private Tuple<Bucket, Bucket> Split(Bucket bucket)
        {
            var split = bucket.Count / 2;

            var bl = new Bucket(bucket.Take(split));
            var br = new Bucket(bucket.Skip(split));
            var idx = buckets.IndexOf(bucket);
            buckets.Insert(idx, bl);
            buckets[idx + 1] = br;

            bucket.Clear();

            return new Tuple<Bucket, Bucket>(bl, br);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            int cc = 0;
            foreach (var bucket in buckets)
            {
                sb.AppendFormat("#{0,-3}(Size:{3,3}) {1} to {2}", cc++, bucket.smallest, bucket.largest, bucket.Count);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public string DebugReport()
        {
            var sb = new StringBuilder();
            int cc = 0;
            sb.AppendLine("Buffer:");
            foreach (var node in buffer)
            {
                sb.Append(", ");
                sb.Append(node.ToString());
            }
            sb.AppendLine();

            sb.AppendLine("Buckets:");
            foreach (var bucket in buckets)
            {
                sb.AppendFormat("#{0,-3}(Size:{3,3}) {1} to {2}", cc++, bucket.smallest, bucket.largest, bucket.Count);
                sb.AppendLine();
                sb.Append("\t");
                foreach (var node in bucket)
                {
                    sb.Append(", ");
                    sb.Append(node.ToString());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

      
    }
}