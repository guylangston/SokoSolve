using System;
using BenchmarkDotNet.Attributes;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;

namespace SokoSolve.Tests.Benchmarks
{
    
    public class SolverNodeLookupBenchmark
    {
        
        [Benchmark]
        public void SolverNodeLookupSimple()
        {
            var simple = new SolverNodeLookupSimple();
            Benchmark(simple, 100000, 16, 16);
        }
        
        [Benchmark]
        public void SolverNodeLookupByBucketWrap()
        {
            var wrap = new SolverNodeLookupThreadSafeWrapper();
            Benchmark(wrap, 100000, 16, 16);
        }

        private void Benchmark(ISolverNodeLookup lookup, int count, int width, int height)
        {
            Random r = new Random();
            for (var x = 0; x < count; x++)
            {
                var n = new SolverNode()
                {
                    CrateMap = new Bitmap(width,height),
                    MoveMap = new Bitmap(width,height),
                };
                for (var y = 0; y < width * height / 4; y++)
                {
                    n.CrateMap[r.Next(0, width), r.Next(0, height)] = true;
                    n.MoveMap[r.Next(0, width), r.Next(0, height)] = true;
                }
                lookup.Add(n);
            }
        }
    }
}