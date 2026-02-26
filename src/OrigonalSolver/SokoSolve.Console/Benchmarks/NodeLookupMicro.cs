using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using SokoSolve.Core.Solver;
using SokoSolve.Core.Solver.Lookup;
using SokoSolve.Core.Util;

namespace SokoSolve.Console.Benchmarks
{
    public class NodeLookupMicro
    {
        private readonly IReadOnlyList<SolverNode> nodes;

        public NodeLookupMicro()
        {
            nodes = SolverNodeBuilder.BuildSolverNodes(10_000).ToImmutableList();
        }

        protected void AddAll(INodeLookup pool)
        {
            foreach (var node in nodes)
            {
                pool.Add(node);
            }
            foreach (var node in nodes)
            {
                pool.FindMatch(node);
            }
        }

        [Benchmark]
        public void AddAll_SortedSet()
        {
            var set = new SortedSet<SolverNode>();
            foreach (var node in nodes)
            {
                set.Add(node);
            }
            foreach (var node in nodes)
            {
                set.Contains(node);
            }
        }

        [Benchmark]
        public void AddAll_obst() => AddAll(new NodeLookupOptimisticLockingBinarySearchTree());

        [Benchmark(Baseline = true)]
        public void AddAll_bst() => AddAll(new NodeLookupBinarySearchTree(new NodeLookupLongTerm()));

        // public void AddAll_def()
        // {
        //     AddAll(new NodeLookupByBucket());
        // }
    }

}
