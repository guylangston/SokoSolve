using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;

namespace SokoSolve.Tests.Benchmarks
{
    
    public class SolverNodeLookupBenchmark
    {
        [GlobalSetup]
        public void Setup()
        {
            this.items = Generate(10_000, 16, 16).ToArray();
        }

        private SolverNode[] items;

        [Benchmark]
        public void SolverNodeLookupSimple()
        {
            var collection = new SolverNodeLookupSimple();
            foreach (var n in items)
            {
                if (collection.FindMatch(n) == null)
                {
                    collection.Add(n);    
                }    
            }
        }
        
        [Benchmark]
        public void SolverNodeLookupByBucketWrap()
        {
            var collection = new SolverNodeLookupThreadSafeWrapper();
            foreach (var n in items)
            {
                if (collection.FindMatch(n) == null)
                {
                    collection.Add(n);    
                }    
            }
        }
        
        // TOO SLOW
        // [Benchmark]
        // public void SolverNodeLookupSimple_Multi()
        // {
        //     var collection = new SolverNodeLookupSimple();
        //
        //     var thread = Environment.ProcessorCount;
        //     var perThread = items.Length / thread;
        //
        //     var locker= new object();
        //     
        //     var tasks = Enumerable.Range(0, thread)
        //               .Select(x => Task.Run(() =>
        //               {
        //                   foreach (var n in items.Skip(x*perThread).Take(perThread))
        //                   {
        //                       lock (locker)
        //                       {
        //                           if (collection.FindMatch(n) == null)
        //                           {
        //                               collection.Add(n);    
        //                           }
        //                       }
        //                   }
        //               })).ToArray();
        //     Task.WaitAll(tasks);
        // }
        
        [Benchmark]
        public void SolverNodeLookupByBucketWrap_Multi()
        {
            var collection = new SolverNodeLookupThreadSafeWrapper();

            var thread = Environment.ProcessorCount;
            var perThread = items.Length / thread;

            var tasks = Enumerable.Range(0, thread)
                                  .Select(x => Task.Run(() =>
                                  {
                                      foreach (var n in items.Skip(x*perThread).Take(perThread))
                                      {
                                          if (collection.FindMatch(n) == null)
                                          {
                                              collection.Add(n);    
                                          }             
                                      }
                                  })).ToArray();
            Task.WaitAll(tasks);
        }

        private IEnumerable<SolverNode> Generate(int count, int width, int height)
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

                yield return n;
            }
        }
    }
}