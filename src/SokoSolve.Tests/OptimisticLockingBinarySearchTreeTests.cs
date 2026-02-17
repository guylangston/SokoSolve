using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using TextRenderZ;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests
{

    public class OptimisticLockingBinarySearchTreeTests
    {

        public class SomeObj : IEquatable<SomeObj>
        {
            public SomeObj(int value)
            {
                Value = value;
            }

            public int Value { get;  }

            public bool Equals(SomeObj? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Value == other.Value;
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((SomeObj)obj);
            }

            public override string ToString() => Value.ToString();
        }

        public class SomeObjComparer : IComparer<SomeObj>
        {
            public int Compare(SomeObj x, SomeObj y) => x.Value.CompareTo(y.Value);
        }

        private readonly ITestOutputHelper outp;
        private  readonly IComparer<SomeObj> cmp = new SomeObjComparer();

        public OptimisticLockingBinarySearchTreeTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        IEnumerable<SomeObj> GenerateRandomSeq(int seed, int count)
        {
            var r = new Random(seed);
            for (int cc = 0; cc < count; cc++)
            {
                yield return new SomeObj(r.Next(0, count*10));
            }
        }

        [Fact]
        public void BuildThenRemove_20()
        {
            BuildThenRemoveInner(GenerateRandomSeq(11, 20).ToList());
        }

        [Fact]
        public void BuildThenRemove_1000()
        {
            BuildThenRemoveInner(GenerateRandomSeq(12, 1000).ToList());
        }

        public void BuildThenRemoveInner(List<SomeObj> items)
        {
            var tree  = new OptimisticLockingBinarySearchTree<SomeObj>(cmp, x=> x.Value/10);
            var dedup = GeneralHelper.DeDup(items);

            // ---[Add]----------------------------
            var dupCount = 0;
            foreach (var item in items)
            {
                if (!tree.TryAdd(item, out var dup))
                {
                    Assert.Contains(dup, dedup.Dups);
                    dupCount++;
                }

                tree.Verify();
            }

            //RenderToGraphVis(tree);

            Assert.Equal(dedup.Dups.Count, dupCount);
            Assert.Equal(dedup.Distinct.Count, tree.Count);

            // Copy to List
            var treeValues = new List<SomeObj>();
            tree.ForEachOptimistic(x=>treeValues.Add(x));

            Assert.True(dedup.Distinct.All(x=>treeValues.Contains(x)));

            // Delete all - in a deterministic order, that is not the creation order (else we always start with root)
            foreach (var item in items.OrderBy(x=>x.Value))
            {
                tree.TryRemove(item);
                tree.Verify();
            }
        }

        [Fact]
        public void WorkedExample() // Mostly just to step through in a debugger
        {
            var tree = new OptimisticLockingBinarySearchTree<SomeObj>(cmp, x=> x.Value/10);

            var input = new int[] { 23, 13, 20, 56, 50, 51, 50, 19, 24 }; // 1 dup
            var items = input.Select(x => new SomeObj(x)).ToList();
            var dedup = GeneralHelper.DeDup(items);

            // ---[Add]----------------------------
            var dupCount = 0;
            foreach (var item in items)
            {
                if (!tree.TryAdd(item, out var dup))
                {
                    Assert.Contains(dup, dedup.Dups);
                    dupCount++;
                }

                tree.Verify();
            }

            RenderToGraphVis(tree);

            Assert.Equal(dedup.Dups.Count, dupCount);
            Assert.Equal(dedup.Distinct.Count, tree.Count);
            Assert.Equal(6-1, tree.CountHashCollision);

            // Copy to List
            var treeValues = new List<SomeObj>();
            tree.ForEachOptimistic(x=>treeValues.Add(x));

            Assert.True(dedup.Distinct.All(x=>treeValues.Contains(x)));

            // ---[Find]----------------------------
            Assert.True(tree.TryFind(new SomeObj(20), out _));
            Assert.True(tree.TryFind(new SomeObj(50), out _));
            Assert.False(tree.TryFind(new SomeObj(-1), out _));
            Assert.False(tree.TryFind(new SomeObj(59), out _));

            // ---[Remove]----------------------------

            Assert.True(tree.TryRemove(new SomeObj(19))); // equal chain
            tree.Verify();
            Assert.False(tree.TryRemove(new SomeObj(19)));

            Assert.True(tree.TryRemove(new SomeObj(13)));   // a left
            tree.Verify();
            Assert.False(tree.TryRemove(new SomeObj(13)));

            //RenderToGraphVis(tree);
            Assert.True(tree.TryRemove(new SomeObj(23))); // Root (many equal hashes)
            tree.Verify();
            Assert.False(tree.TryRemove(new SomeObj(23)));

            //RenderToGraphVis(tree);
            Assert.True(tree.TryRemove(new SomeObj(24))); // Root (many equal hashes)
            tree.Verify();
            Assert.False(tree.TryRemove(new SomeObj(24)));

            Assert.True(tree.TryRemove(new SomeObj(20)));
            tree.Verify();
            Assert.False(tree.TryRemove(new SomeObj(20)));
        }

        public void RenderToGraphVis(OptimisticLockingBinarySearchTree<SomeObj> tree)
        {
            var sb = new FluentString();
            sb.AppendLine("digraph g {");
            sb.AppendLine("rankdir=TB;");

            var nodes = new List<OptimisticLockingBinarySearchTree<SomeObj>.Node>();
            tree.ForEachNode(x=>nodes.Add(x));
            foreach (var node in nodes)
            {
                if (node.Left != null)    sb.AppendLine($"\t{node.Value}->{node.Left.Value}[color=\"green\"]");

                foreach (var hash in node.ForeachSameHash())
                {
                    if (object.ReferenceEquals(hash, node.Value)) continue;

                    sb.AppendLine($"\t{node.Value}->{hash.Value}[color=\"gray\"]");

                    sb.AppendLine($"\t{hash.Value}[color=\"gray\"]");
                }

                if (node.Right != null)   sb.AppendLine($"\t{node.Value}->{node.Right.Value}[color=\"red\"]");
            }

            sb.AppendLine("}");

            outp.WriteLine(sb);
        }

        [Fact]
        public void AddThenRetrieve()
        {
            var tree = new OptimisticLockingBinarySearchTree<SomeObj>(cmp, x=> x.Value/10);

            var items = GenerateRandomSeq(1, 100).ToList();
            var dedup = GeneralHelper.DeDup(items);

            var dups = new List<SomeObj>();
            foreach (var item in items)
            {
                if (!tree.TryAdd(item, out var dup))
                {
                    Assert.Contains(dup, dedup.Dups);
                    dups.Add(dup);
                }
            }

            //RenderToGraphVis(tree);

            // Copy to List
            var treeValues = new List<SomeObj>();
            tree.ForEachOptimistic(x=>treeValues.Add(x));

            Assert.True(dedup.Distinct.All(x=>treeValues.Contains(x)));

            Assert.Equal(dedup.Dups.Count, dups.Count);
            Assert.Equal(dedup.Distinct.Count, tree.Count);

            foreach (var item in items)
            {
                Assert.True(tree.TryFind(item, out _));
            }
        }

        private class Job
        {
            private OptimisticLockingBinarySearchTree<SomeObj> tree;
            public List<SomeObj> ToAdd { get; }
            public Job(OptimisticLockingBinarySearchTree<SomeObj> tree, List<SomeObj> toAdd)
            {
                this.tree  = tree;
                this.ToAdd = toAdd;
            }

            public int dups;

            public void Run()
            {
                foreach (var item in ToAdd)
                {
                    if (!tree.TryAdd(item, out _))
                    {
                        dups++;
                    }
                }
            }
        }

        [Fact]
        public void AddThenRetrieve_100_000()
            => AddThenRetrieveAsync(GenerateRandomSeq(1, 100_000), x=>x.Value/2, 1);

        [Fact]
        public void AddThenRetrieveAsync_100_000()
            => AddThenRetrieveAsync(GenerateRandomSeq(143, 100_000), x=>x.Value/2, Environment.ProcessorCount);

        [Fact]
        public void AddThenRetrieveAsync_1000()
            => AddThenRetrieveAsync(GenerateRandomSeq(1, 1000), x=>x.Value/2, Environment.ProcessorCount);

        [Fact]
        public void AddThenRetrieveAsync2_100_000()
            => AddThenRetrieveAsync(GenerateRandomSeq(1, 100_000), x=>x.Value/2, 2);

        public void AddThenRetrieveAsync(IEnumerable<SomeObj> workList, Func<SomeObj, int> hasher, int workers)
        {
            outp.WriteLine("Starting");
            // Prep
            var tree  = new OptimisticLockingBinarySearchTree<SomeObj>(cmp, hasher);
            var items = workList.ToList();

            // Workers

            outp.WriteLine("Seeding");
            var tasks = new List<Task>();
            var jobs = new List<Job>();

            for (int cc = 0; cc < workers; cc++)
            {
                var state = new Job(tree, new List<SomeObj>());
                jobs.Add(state);
                var t     = new Task(x=>(x as Job).Run(),  state);
                tasks.Add(t);
            }

            // Safe but slow way of distributing the work
            var i     = 0;
            var queue = new Queue<SomeObj>(items);
            while (queue.Count > 0)
            {
                jobs[i].ToAdd.Add(queue.Dequeue());
                i++;
                if (i >= jobs.Count) i = 0;
            }

            Assert.Equal(0, queue.Count);

            outp.WriteLine("Running");
            foreach (var task in tasks)
            {
                task.Start();
            }

            outp.WriteLine("Waiting");

            Task.WaitAll(tasks.ToArray());

            Assert.True(tasks.All(x=>x.IsCompletedSuccessfully));

            outp.WriteLine("Checking");
            tree.Verify();

            foreach (var item in items.WithIndex())
            {
                Assert.True(tree.TryFind(item.item, out _), item.ToString());
            }

        }

    }
}
