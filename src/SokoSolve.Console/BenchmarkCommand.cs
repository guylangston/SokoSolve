using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Reporting;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console
{
    internal static class BenchmarkCommand
    {
        
        public static Command GetCommand()
        {
            var bench = new Command("benchmark", "Benchmark a single puzzle")
            {
                new Argument<string>( () => "SQ1~P5")
                {
                    Name        = "puzzle",
                    Description = "Puzzle Identifier in the form LIB~PUZ (can be regex)"
                },
                new Option<int>(new[] {"--min", "-m"}, "TimeOut in minutes")
                {
                    Name = "min",
                },
                new Option<int>(new[] {"--sec", "-s"}, "TimeOut in seconds")
                {
                    Name = "sec",
                },
                new Option<string>(new[] {"--solver", "-t"}, "Solver Strategy: "+SolverFactoryHelp)
                {
                    Name = "solver",
                },
                new Option<string>(new[] {"--pool", "-p"}, "ISolverPool Type")
                {
                    Name = "pool",
                }, 
                new Option<double>(new[] {"--max-rating", "-maxR"},  "Max Puzzle Rating")
                {
                    Name = "maxR",
                },
                new Option<double>(new[] {"--min-rating", "-minR"},  "Min Puzzle Rating")
                {
                    Name = "minR",
                }
            }; 
            bench.Handler = CommandHandler.Create<string, int, int, 
                string, string,
                double, double>(Run);
            return bench;
        }
        
        public static void Run(
            string puzzle = "SQ1~P5", int min = 0, int sec = 0, 
            string solver = "fr!", string pool = PoolDefault, 
            double minR = 0, double maxR = 2000)
        {
            if (min == 0 && sec == 0) min = 3;
            
            var pathHelper = new PathHelper();
            var compLib = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));

            var selection = compLib.GetPuzzlesWithCachingUsingRegex(puzzle).ToArray();
            if (!selection.Any())
            {
                throw new Exception($"Not puzzles found '{puzzle}', should be SQ1~P5 or SQ1, etc"); 
            }
            
            var solverRun = new SolverRun();
            solverRun.Init();
            solverRun.AddRange(
                selection
                  .OrderBy(x=>x.Rating)
                  .Where(x=>x.Rating >= minR && x.Rating <= maxR)
            );
            
            SolverRun(puzzle, min, sec, solver,pool, minR, maxR, solverRun);
        }

      public static int SolverRun(string puzzle,
            int min, int sec,
            string solver, string pool,
            double minR, double maxR,
            SolverRun solverRun)
      {
          var args =
              new FluentStringBuilder(" ")
                  .Append(puzzle).Sep()
                  .Append($"--solver {solver}").Sep()
                  .Append($"--pool {pool}").Sep()
                  .If(min > 0, $"--min {min}").Sep()
                  .If(sec > 0, $"--sec {sec}").Sep()
                  .If(minR > 0, $"--min-rating {minR}").Sep()
                  .If(maxR < 2000, $"--min-rating {maxR}");
            
            var exitRequested = false;
            SolverCommand? executing = null;
            
            // Setup: Report and cancellation 
            var outFile   = $"./benchmark--{DateTime.Now:s}.txt".Replace(':', '-');
            var outFolder = "./results/";
            if (!Directory.Exists(outFolder)) Directory.CreateDirectory(outFolder);
            var info = new FileInfo(Path.Combine(outFolder, outFile));
            

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
                    AggProgress = new ConsoleProgressNotifier(TextWriter.Null),  
                    CheckAbort  = x => exitRequested
                };

                var runner         = new BatchSolveComponent(report, System.Console.Out);
                var solverInstance = SolverFactory(strat.Solver, ioc);
                var summary = runner.Run(solverRun, solverCommand, solverInstance, false);
                results.Add((strat, summary));
            }
            
            // Header
            var extras = new Dictionary<string, string>()
            {
                {"Args", args},
                {"Report", info.FullName }
            };
            DevHelper.WriteFullDevelopmentContext(report, extras);
            DevHelper.WriteFullDevelopmentContext(System.Console.Out, extras);
            
            // Body
            var reportRow = GenerateReport(results);
            MapToReporting.Create<SummaryLine>()
                          .AddColumn("Solver", x=>x.Strategy.Solver)
                          .AddColumn("Pool", x=>x.Strategy.Pool)
                          .AddColumn("Puzzle", x=>x.Result.Puzzle.Ident)
                          .AddColumn("Result", x=>x.Result.Exited)
                          .AddColumn("Solutions", x=>(x.Result.Solutions?.Count ?? 0) == 0 ? null : (int?)x.Result.Solutions.Count)
                          .AddColumn("Statistics", x=>
                              x.Result.Exited == ExitConditions.Conditions.Error 
                                  ? x.Result.Exited.ToString()
                                  : x.Result.Statistics?.ToString(false, true)
                                  )
                          .RenderTo(report, reportRow)
                          .RenderTo(System.Console.Out, reportRow);
            
            return results.Any(x => x.Item2.Any(y=>y.Exited == ExitConditions.Conditions.Error)) ? -1 : 0; // All exceptions
        }

      private static IEnumerable<SummaryLine> GenerateReport(List<(Strategy, List<SolverResultSummary>)> results)
      {
          foreach (var (strategy, runResult) in results)
          {
              foreach (var rr in runResult)
              {
                    yield return new SummaryLine()
                    {
                        Strategy = strategy,
                        Result = rr
                    };
                  // line = $"-> {strategy.Solver,-4} {strategy.Pool,-12} {rr.Puzzle.Ident,7} | {rr.Text}";
                  // report.WriteLine(line); System.Console.WriteLine(line);
              }
          }
      }

      public class SummaryLine
      {
          public Strategy Strategy { get; set; }
          public SolverResultSummary Result { get; set; }
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

        public const string PoolDefault = "bb:lock:sl:lt";
        public const string PoolFactoryAll = "bb:ll:lt,bb:lock:bucket,bb:bucket,lock:bucket,bucket,baseline";
        public const string PoolFactoryHelp = PoolFactoryAll;
        private static ISolverPool PoolFactory(string pool)
        {
            return pool switch
            {
                // New work
                "bb:ll:lt" => new SolverPoolDoubleBuffered(new SolverPoolSortedLinkedList(new SolverPoolLongTerm())),
                "bb:lock:ll:lt" => new SolverPoolDoubleBuffered(new SolverPoolSlimRwLock(new SolverPoolSortedLinkedList(new SolverPoolLongTerm()))),
                "bb:lock:sl:lt" => new SolverPoolDoubleBuffered(new SolverPoolSlimRwLock(new SolverPoolSortedList(new SolverPoolLongTerm()))),
                "bb:bst:lt" => new SolverPoolDoubleBuffered(new SolverPoolBinarySearchTree(new SolverPoolLongTerm())),
                "bb:lock:bst:lt" => new SolverPoolDoubleBuffered(new SolverPoolSlimRwLock(new SolverPoolBinarySearchTree(new SolverPoolLongTerm()))),
                
                // Older
                "bb:lock:bucket" => new SolverPoolDoubleBuffered( new SolverPoolSlimRwLock(new SolverPoolByBucket())),
                "bb:bucket" => new SolverPoolDoubleBuffered(new SolverPoolByBucket()),
                "lock:bucket" => new SolverPoolSlimRwLock(new SolverPoolByBucket()),
                "bucket" => new SolverPoolByBucket(),


                // Just for comparison, never really intended for use
                "baseline"  => new SolverPoolSlimRwLock(new SolverPoolSimpleList()),
                
                _ => throw new Exception($"Unknown Pool '{pool}', try ({PoolFactoryHelp})")
            };
        }

        public const string SolverFactoryHelp = "f, r, fr, fr!";
        private static ISolver SolverFactory(string solver, SolverContainerByType ioc)
        {
            return solver switch
            {
                "f"   => new SingleThreadedForwardSolver(new SolverNodeFactoryTrivial()),
                "r"   => new SingleThreadedReverseSolver(new SolverNodeFactoryTrivial()),
                "fr"  => new SingleThreadedForwardReverseSolver(new SolverNodeFactoryTrivial()),
                "fr!" => new MultiThreadedForwardReverseSolver(new SolverNodeFactoryTrivial()),
                "fr!p" => new MultiThreadedForwardReverseSolver(new SolverNodeFactoryPoolingConcurrentBag()),
                "fr!P" => new MultiThreadedForwardReverseSolver(new SolverNodeFactoryPooling()),
                _     => throw new Exception($"Unknown Solver '{solver}', try ({SolverFactoryHelp})")
            };
        }
    }
    
}