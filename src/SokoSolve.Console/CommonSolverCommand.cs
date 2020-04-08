using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console
{
    internal static class CommonSolverCommand
    {
        public static int SolverRun(
            int min, int sec,
            string solver, string pool,
            double minR, double maxR,
            SolverRun solverRun)
        {
            System.Console.WriteLine($"   Args| --min {min} --sec {sec} --solver {solver} --pool {pool} --min-rating {minR} --max-ratring {maxR}");
            
            var exitRequested = false;
            SolverCommand? executing = null;
            
            // Setup: Report and cancellation 
            var outFile   = $"./benchmark--{DateTime.Now:s}.txt".Replace(':', '-');
            var outFolder = "./results/";
            if (!Directory.Exists(outFolder)) Directory.CreateDirectory(outFolder);
            var info = new FileInfo(Path.Combine(outFolder, outFile));
            System.Console.WriteLine($" Report| {info.FullName}");
            System.Console.WriteLine();

            using var report = File.CreateText(info.FullName);
            System.Console.CancelKeyPress += (o, e) =>
            {
                report.Flush();
                System.Console.WriteLine("Ctrl+C detected; cancel requested");

                if (executing != null)
                {
                    executing.ExitConditions.ExitRequested = true;    
                }
                exitRequested = true;
            };

            var results = new List<(Strategy, List<SolverResultSummary>)>(); 
            var perm = GetPermutations(solver, pool).ToList();
            var countStrat = 0;
            foreach(var strat in perm)
            {
                if (perm.Count > 1)
                {
                    countStrat++;
                    System.Console.WriteLine($"[Strategy {countStrat}/{perm.Count}] {strat}");
                }
                
                var ioc = new SolverContainerByType(new Dictionary<Type, Func<Type, object>>()
                {
                    {typeof(ISolverPool),      _ => PoolFactory(strat.Pool)},
                    {typeof(ISolverQueue),     _ => new SolverQueueConcurrent()},
                });
                var solverCommand = new SolverCommand
                {
                    ServiceProvider = ioc,
                    ExitConditions = new ExitConditions()
                    {
                        Duration       = TimeSpan.FromMinutes(min).Add(TimeSpan.FromSeconds(sec)),
                        StopOnSolution = true,
                    },
                    AggProgress = new ConsoleProgressNotifier(),  
                    CheckAbort  = x => exitRequested
                };

                var runner         = new BatchSolveComponent(report, System.Console.Out);
                var solverInstance = SolverFactory(strat.Solver, ioc);
                var summary = runner.Run(solverRun, solverCommand, solverInstance, false);
                results.Add((strat, summary));
            }
            
            var cc = 0;
            var line = DevHelper.FullDevelopmentContext();
            report.WriteLine(line); System.Console.WriteLine(line);
            
            foreach (var (strategy, runResult) in results)
            {
                foreach (var rr in runResult)
                {
                    line = $"-> {strategy.Solver,-4} {strategy.Pool,-12} {rr.Puzzle.Ident,7} | {rr.Text}";
                    report.WriteLine(line); System.Console.WriteLine(line);
                }
            }
            
            return 0; //return summ.All(x => x.Solutions.Any()) ? 0 : -1; // All solutions
        }
        
        

        public class Strategy
        {
            public Strategy(string solver, string pool)
            {
                Solver = solver;
                Pool = pool;
            }

            public string Solver { get;  }
            public string Pool   { get;  }

            public override string ToString() => $"Solver={Solver,-8} Pool={Pool,-8}";

            public string ToStringShort() => $"{Solver}|{Pool}";
        }

        private static IEnumerable<Strategy> GetPermutations(string solver, string pool)
        {
            if (pool == "all") pool = PoolFactoryAll;
            foreach(var s in solver.Split(','))
                foreach (var p in pool.Split(','))
                {
                    yield return new Strategy(s, p);
                }
        }

        public const string PoolFactoryAll = "bb:bucket,bb:ll:lt,bucket,baseline";
        public const string PoolFactoryHelp = "bb:bucket,bb:ll:lt,bucket,legacy,baseline";
        private static ISolverPool PoolFactory(string pool)
        {
            return pool switch
            {
                "bb:bucket" => new SolverPoolDoubleBuffered(new SolverPoolByBucket()),
                "bb:ll:lt"  => new SolverPoolDoubleBuffered(new SolverPoolSortedLinkedList(new SolverPoolLongTerm())),
                "bucket"    => new SolverPoolByBucket(),
                "legacy"    => new SolverPoolByBucket(),
                "baseline"  => new SolverPoolSlimRwLock(new SolverPoolSimpleList()),
                
                _ => throw new Exception($"Unknown Pool '{pool}', try ({PoolFactoryHelp})")
            };
        }

        public const string SolverFactoryHelp = "f, r, fr, fr!";
        private static ISolver SolverFactory(string solver, SolverContainerByType ioc)
        {
            return solver switch
            {
                "f"   => new SingleThreadedForwardSolver(),
                "r"   => new SingleThreadedReverseSolver(),
                "fr"  => new SingleThreadedForwardReverseSolver(),
                "fr!" => new MultiThreadedForwardReverseSolver(),
                _     => throw new Exception($"Unknown Solver '{solver}', try ({SolverFactoryHelp})")
            };
        }
    }
}