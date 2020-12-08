using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib.DB;
using TextRenderZ;
using TextRenderZ.Reporting;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
   

    public class BatchSolveComponent
    {
        public class BatchArgs
        {
            public BatchArgs(string puzzle, int min, int sec, string solver, string pool, double minR, double maxR, string save, ITextWriter console)
            {
                Puzzle       = puzzle;
                Min          = min;
                Sec          = sec;
                Solver       = solver;
                Pool         = pool;
                MinR         = minR;
                MaxR         = maxR;
                Save         = save;
                Console = console;
            }

            public string      Puzzle  { get; }
            public int         Min     { get; }
            public int         Sec     { get; }
            public string      Solver  { get; }
            public string      Pool    { get; }
            public double      MinR    { get; }
            public double      MaxR    { get; }
            public string      Save    { get; }
            public ITextWriter Console { get; }
        }

        public bool CatReport { get; set; }

        public int SolverRun(BatchArgs batchArgs, SolverRun run)
        {
            var args =
                new FluentString(" ")
                    .Append(batchArgs.Puzzle).Sep()
                    .Append($"--solver {batchArgs.Solver}").Sep()
                    .Append($"--pool {batchArgs.Pool}").Sep()
                    .If(batchArgs.Min > 0, $"--min {batchArgs.Min}").Sep()
                    .If(batchArgs.Sec > 0, $"--sec {batchArgs.Sec}").Sep()
                    .If(batchArgs.MinR > 0, $"--min-rating {batchArgs.MinR}").Sep()
                    .If(batchArgs.MaxR < 2000, $"--min-rating {batchArgs.MaxR}");
          
            batchArgs.Console.WriteLine($"Arguments: {args}");
            
            var            exitRequested = false;
            SolverCommand? executing     = null;
            
            // Setup: Report and cancellation 
            var benchId   = DateTime.Now.ToString("s").Replace(':', '-');
            var outFile   = $"./benchmark--{benchId}.txt";
            var outTele   = $"./telemetry--{benchId}.csv";
            var outFolder = "./results/";
            if (!Directory.Exists(outFolder)) Directory.CreateDirectory(outFolder);
            var info = new FileInfo(System.IO.Path.Combine(outFolder, outFile));
            var tele = new FileInfo(System.IO.Path.Combine(outFolder, outTele));
            
            var results = new List<(Strategy, List<SolverResultSummary>)>();

            using var report  = File.CreateText(info.FullName);
            using var repTele = File.CreateText(tele.FullName);
            
                System.Console.CancelKeyPress += (o, e) =>
                {
                    report.Flush();
                    batchArgs.Console.WriteLine("Ctrl+C detected; cancel requested");

                    if (executing != null)
                    {
                        executing.ExitConditions.ExitRequested = true;    
                    }
                    exitRequested = true;
                };
                
                ISokobanSolutionRepository? solutionRepo = File.Exists("./solutions.json") && !DevHelper.IsDebug()
                    ? new JsonSokobanSolutionRepository("./solutions.json")
                    : null;
                ISolverRunTracking? runTracking = null;

                 
                var perm       = GetPermutations(batchArgs.Solver, batchArgs.Pool).ToList();
                var countStrat = 0;
                foreach(var strat in perm)
                {
                    countStrat++;
                    batchArgs.Console.WriteLine($"(Strategy {countStrat}/{perm.Count}) {strat}");
                    
                    var ioc = new SolverContainerByType(new Dictionary<Type, Func<Type, object>>()
                    {
                        {typeof(ISolverPool),      _ => PoolFactory(strat.Pool)},
                        {typeof(ISolverQueue),     _ => new SolverQueueConcurrent()},
                        {typeof(ISolverRunTracking), _ => runTracking},
                        {typeof(ISokobanSolutionRepository), _ => solutionRepo},
                    });
                    var solverCommand = new SolverCommand
                    {
                        ServiceProvider = ioc,
                        ExitConditions = new ExitConditions()
                        {
                            Duration       = TimeSpan.FromMinutes(batchArgs.Min).Add(TimeSpan.FromSeconds(batchArgs.Sec)),
                            MemAvail       = DevHelper.GiB_1 /2, // Stops the machine hanging / swapping to death
                            StopOnSolution = true,
                        },
                        AggProgress = new ConsoleProgressNotifier(repTele),  
                        CheckAbort  = x => exitRequested,
                    };

                   
                    var runner = new SingleSolverBatchSolveComponent(
                        new TextWriterAdapter(report), 
                        batchArgs.Console, 
                        solutionRepo, 
                        runTracking, 
                        5, 
                        false);
                    
                    var solverInstance = SolverFactory(strat.Solver, ioc);
                    var summary= runner.Run(run, solverCommand, solverInstance, false, batchArgs);
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
                var reportRow = GenerateReport(results).ToList();
                MapToReporting.Create<SummaryLine>()
                              .AddColumn("Solver", x=>x.Strategy.Solver)
                              .AddColumn("Pool", x=>x.Strategy.Pool)
                              .AddColumn("Puzzle", x=>x.Result.Puzzle.Ident)
                              .AddColumn("State", x=>x.Result.Exited)
                              .AddColumn("Solutions", x=>(x.Result.Solutions?.Count ?? 0) == 0 ? null : (int?)x.Result.Solutions.Count)
                              .AddColumn("Statistics", x=>
                                  x.Result.Exited == ExitConditions.Conditions.Error 
                                      ? x.Result.Exited.ToString()
                                      : x.Result.Statistics?.ToString(false, true)
                              )
                              .RenderTo(reportRow, new MapToReportingRendererText(), report)
                              .RenderTo(reportRow, new MapToReportingRendererText(), System.Console.Out);
                
            

            // if (CatReport)
            // {
            //     System.Console.WriteLine("========================================================================");
            //     System.Console.WriteLine(File.ReadAllText(info.FullName));
            //     
            // }
            
            
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
                        Result   = rr
                    };
                    // line = $"-> {strategy.Solver,-4} {strategy.Pool,-12} {rr.Puzzle.Ident,7} | {rr.Text}";
                    // report.WriteLine(line); System.Console.WriteLine(line);
                }
            }
        }

        public class SummaryLine
        {
            public Strategy            Strategy { get; set; }
            public SolverResultSummary Result   { get; set; }
        }
        
        public class Strategy
        {
            public Strategy(string solver, string pool)
            {
                Solver = solver;
                Pool   = pool;
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

        public const string PoolDefault     = "lock:bst:lt";
        public const string PoolBaseline    = "bb:lock:sl:lt";
        public const string PoolFactoryAll  = "bb:ll:lt,bb:lock:bucket,bb:bucket,lock:bucket,bucket,baseline";
        public const string PoolFactoryHelp = PoolFactoryAll;
        public const string SolverFactoryHelp = "f, r, fr, fr!";
        
        
        public ISolverPool PoolFactory(string pool)
        {
            return pool switch
            {
                // New work
                "bb:ll:lt"       => new SolverPoolDoubleBuffered(new SolverPoolSortedLinkedList(new SolverPoolLongTerm())),
                "bb:lock:ll:lt"  => new SolverPoolDoubleBuffered(new SolverPoolSlimRwLock(new SolverPoolSortedLinkedList(new SolverPoolLongTerm()))),
                "bb:lock:sl:lt"  => new SolverPoolDoubleBuffered(new SolverPoolSlimRwLock(new SolverPoolSortedList(new SolverPoolLongTerm()))),
                "bb:bst:lt"      => new SolverPoolDoubleBuffered(new SolverPoolBinarySearchTree(new SolverPoolLongTerm())),
                "bb:lock:bst:lt" => new SolverPoolDoubleBuffered(new SolverPoolSlimRwLock(new SolverPoolBinarySearchTree(new SolverPoolLongTerm()))),
                "lock:bst:lt"    => new SolverPoolSlimRwLock(new SolverPoolBinarySearchTree(new SolverPoolLongTerm())),
                
                // Older
                "bb:lock:bucket" => new SolverPoolDoubleBuffered( new SolverPoolSlimRwLock(new SolverPoolByBucket())),
                "bb:bucket"      => new SolverPoolDoubleBuffered(new SolverPoolByBucket()),
                "lock:bucket"    => new SolverPoolSlimRwLock(new SolverPoolByBucket()),
                "bucket"         => new SolverPoolByBucket(),

                // Just for comparison, never really intended for use
                "baseline" => new SolverPoolSlimRwLock(new SolverPoolSimpleList()),
                
                _ => throw new Exception($"Unknown Pool '{pool}', try ({PoolFactoryHelp})")
            };
        }

        
        public ISolver SolverFactory(string solver, SolverContainerByType ioc)
        {
            return solver switch
            {
                "f"     => new SingleThreadedForwardSolver(new SolverNodeFactoryTrivial()),
                "r"     => new SingleThreadedReverseSolver(new SolverNodeFactoryTrivial()),
                "fr"    => new SingleThreadedForwardReverseSolver(new SolverNodeFactoryTrivial()),
                "fr!"   => new MultiThreadedForwardReverseSolver(new SolverNodeFactoryTrivial()),
                "fr!p"  => new MultiThreadedForwardReverseSolver(new SolverNodeFactoryPoolingConcurrentBag()),
                "fr!py" => new MultiThreadedForwardReverseSolver(new SolverNodeFactoryPoolingConcurrentBag("index")),
                "fr!pz" => new MultiThreadedForwardReverseSolver(new SolverNodeFactoryPoolingConcurrentBag("byteseq")),
                "fr!P"  => new MultiThreadedForwardReverseSolver(new SolverNodeFactoryPooling()),
                "f!pz"  => new MultiThreadedForwardReverseSolver(new SolverNodeFactoryPoolingConcurrentBag("byteseq"))
                {
                    ThreadCountReverse = 1,
                    ThreadCountForward = Environment.ProcessorCount
                },
                "fr!pz11" => new MultiThreadedForwardReverseSolver(new SolverNodeFactoryPoolingConcurrentBag("byteseq"))
                {
                    ThreadCountReverse = 1,
                    ThreadCountForward = 1
                },
                _ => throw new Exception($"Unknown Solver '{solver}', try ({SolverFactoryHelp})")
            };
        }

    }
}