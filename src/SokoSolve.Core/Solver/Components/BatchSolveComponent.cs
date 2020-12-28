using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using TextRenderZ.Reporting;

namespace SokoSolve.Core.Solver.Components
{
    public class BatchSolveComponent
    {
        private LibraryComponent compLib;
        private ITextWriter progress;
        private ISokobanSolutionRepository? repSolutions;
        private ISokobanSolutionComponent? compSolutions;
        private ISolverRunTracking? runTracking;

        public BatchSolveComponent(LibraryComponent compLib, ITextWriter progress, ISokobanSolutionRepository? repSolutions)
        {
            this.compLib      = compLib;
            this.progress      = progress;
            this.repSolutions = repSolutions;
        }

        public bool CatReport { get; set; }

        public int SolverRun(SolverRun run, Dictionary<string, string> solverArgs)
        {
            // var args =
            //     new FluentString(" ")
            //         .Append(batchArgs.Puzzle).Sep()
            //         .Append($"--solver {batchArgs.Solver}").Sep()
            //         .Append($"--pool {batchArgs.Pool}").Sep()
            //         .If(batchArgs.Min > 0, $"--min {batchArgs.Min}").Sep()
            //         .If(batchArgs.Sec > 0, $"--sec {batchArgs.Sec}").Sep()
            //         .If(batchArgs.MinR > 0, $"--min-rating {batchArgs.MinR}").Sep()
            //         .If(batchArgs.MaxR < 2000, $"--min-rating {batchArgs.MaxR}");
            // progress.WriteLine($"Arguments: {args}");
            
            var            exitRequested = false;
            SolverCommand? executing     = null;
            
            // Setup: Report and cancellation 
            var benchId   = DateTime.Now.ToString("s").Replace(':', '-');
            var outFile   = $"./SokoSolve--{benchId}.txt";
            //var outTele   = $"./SokoSolve--{benchId}.csv";
            var outFolder = "./results/";
            if (!Directory.Exists(outFolder)) Directory.CreateDirectory(outFolder);
            var info = new FileInfo(Path.Combine(outFolder, outFile));
            //var tele = new FileInfo(Path.Combine(outFolder, outTele));

            var builder = new SolverBuilder(compLib, repSolutions);
            var results = new List<(Strategy, List<SolverResultSummary>)>();

            using (var report = File.CreateText(info.FullName))
            {
              //   using var repTele = File.CreateText(tele.FullName);
            
                System.Console.CancelKeyPress += (o, e) =>
                {
                    report.Flush();
                    progress.WriteLine("");
                    progress.WriteLine("");
                    progress.WriteLine("Ctrl+C detected; cancel requested");
                    progress.WriteLine($"Report: {info} (+ .csv)");

                    if (executing != null)
                    {
                        executing.ExitConditions.ExitRequested = true;    
                    }
                    exitRequested = true;
                };
                
                var perm       = GetPermutations(solverArgs["solver"], solverArgs["pool"], solverArgs["queue"]).ToList();
                var countStrat = 0;
                foreach(var strat in perm)
                {
                    countStrat++;
                    progress.WriteLine($"(Strategy {countStrat}/{perm.Count}) {strat}");

                    var runArgs = new Dictionary<string, string>(solverArgs);  // copy
                    runArgs["solver"] = strat.Solver;
                    runArgs["pool"]   = strat.Pool;
                    runArgs["queue"]   = strat.Queue;

                    var runner = new SingleSolverBatchSolveComponent(
                        new TextWriterAdapter(report), 
                        progress, 
                        compSolutions, 
                        runTracking, 
                        5, 
                        false);
                    var summary = runner.SolveOneSolverManyPuzzles(run, false, builder, runArgs);
                    results.Add((strat, summary));
                }
                
                // Header
                var extras = new Dictionary<string, string>()
                {
                   // {"Args", args},
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
                                  x.Result.Exited == ExitResult.Error 
                                      ? x.Result.Exited.ToString()
                                      : x.Result.Statistics?.ToString(false, true)
                              )
                              .RenderTo(reportRow, new MapToReportingRendererText(), report)
                              .RenderTo(reportRow, new MapToReportingRendererText(), System.Console.Out);
            }
           
            if (CatReport)
            {
                System.Console.WriteLine("========================================================================");
                System.Console.WriteLine(File.ReadAllText(info.FullName));
                
            }
            
            return results.Any(x => x.Item2.Any(y=>y.Exited == ExitResult.Error)) ? -1 : 0; // All exceptions
           
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
            public Strategy(string solver, string pool, string queue)
            {
                Solver = solver;
                Pool   = pool;
                Queue  = queue;
            }

            public string Solver { get;  }
            public string Pool   { get;  }
            public string Queue   { get;  }

            public override string ToString() => $"Solver={Solver,-12} | Queue={Queue,-12} | Pool={Pool,-12}";

            public string ToStringShort() => $"{Solver}|{Pool}|{Queue}";
        }

        private static IEnumerable<Strategy> GetPermutations(string solver, string pool, string queue)
        {
            if (pool == "all")
            {
                foreach (var lookupObj in SolverBuilder.LookupFactory.GetAllKeys())
                {
                    foreach (var qKey in SolverBuilder.QueueFactory.GetAllKeys())
                    {
                        yield return new Strategy(solver, lookupObj, qKey);    
                    }
                    
                }
            }
            else
            {
                foreach(var s in solver.Split(','))
                foreach (var p in pool.Split(','))
                foreach (var q in queue.Split(','))
                {
                    yield return new Strategy(s, p, q);
                }    
            }
            
        }
    }
}