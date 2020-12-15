using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Components;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using TextRenderZ;
using TextRenderZ.Reporting;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    public interface ISolverRunTracking
    {
        void Begin(SolverState command);
        void End(SolverState state);
    }
    
    /// <summary>
    ///     For memory reasons, we cannot allow ANY state from the Solver.
    ///     This would cause out of memory issues.
    /// </summary>
    public class SolverResultSummary
    {
        public SolverResultSummary(LibraryPuzzle puzzle, List<Path> solutions, ExitConditions.Conditions exited, string text, TimeSpan duration, SolverStatistics statistics)
        {
            Puzzle = puzzle;
            Solutions = solutions;
            Exited = exited;
            Text = text;
            Duration = duration;
            Statistics = statistics;
        }

        public LibraryPuzzle             Puzzle     { get;  }
        public List<Path>?               Solutions  { get; }
        public ExitConditions.Conditions Exited     { get;  }
        public string                    Text       { get;  }
        public TimeSpan                  Duration   { get;  }
        public SolverStatistics          Statistics { get;  }
    }

    public class SingleSolverBatchSolveComponent
    {
        public SingleSolverBatchSolveComponent(ITextWriter report, ITextWriter progress, ISokobanSolutionComponent? compSolutions, ISolverRunTracking? tracking, int stopOnConsecutiveFails, bool skipPuzzlesWithSolutions)
        {
            Report = report;
            Progress = progress;
            CompSolutions = compSolutions;
            Tracking = tracking;
            StopOnConsecutiveFails = stopOnConsecutiveFails;
            SkipPuzzlesWithSolutions = skipPuzzlesWithSolutions;
        }

        public SingleSolverBatchSolveComponent(ITextWriter report, ITextWriter progress)
        {
            Report = report;
            Progress = progress;
            CompSolutions = null;
            Tracking = null;
            StopOnConsecutiveFails = 5;
            SkipPuzzlesWithSolutions = false;
        }

        public ITextWriter                Report                   { get; }
        public ITextWriter                Progress                 { get; }
        public ISokobanSolutionComponent? CompSolutions               { get; }
        public ISolverRunTracking?        Tracking                 { get; }
        public int                        StopOnConsecutiveFails   { get; }
        public bool                       SkipPuzzlesWithSolutions { get; }
        public bool                       WriteSummaryToConsole    { get; set; } = true;

        public List<SolverResultSummary> Run(
            SolverRun run, 
            SolverCommand baseCommand, 
            ISolver solver,
            bool showSummary,
            BatchSolveComponent.BatchArgs? batchArgs = null)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (baseCommand == null) throw new ArgumentNullException(nameof(baseCommand));
            if (solver == null) throw new ArgumentNullException(nameof(solver), "See: " + nameof(SingleThreadedForwardSolver));

            Report.WriteLine("Puzzle Exit Conditions: {0}", run.PuzzleExit);
            Report.WriteLine("Batch Exit Conditions : {0}", run.BatchExit);
            Report.WriteLine("Environment           : {0}", DevHelper.RuntimeEnvReport());
            Report.WriteLine("Solver Environment    : v{0} -- {1}", SolverHelper.VersionUniversal, SolverHelper.VersionUniversalText);
            Report.WriteLine("Started               : {0}", DateTime.Now.ToString("u"));
            Report.WriteLine();

            var res = new List<SolverResultSummary>();
            var start = new SolverStatistics
            {
                Started = DateTime.Now
            };
            SolverState? state = null;
            var pp = 0;
            var consecutiveFails = 0;
            foreach (var puzzle in run)
            {
                
                if (baseCommand.CheckExit(null, out var exit))
                {
                    Progress.WriteLine($"EXITING...{exit}");
                    break;
                }

                try
                {
                    pp++;
                    
                    Progress.WriteLine($"(Puzzle   {pp}/{run.Count}) Attempting: {puzzle.Ident} \"{puzzle.Name}\", R={StaticAnalysis.CalculateRating(puzzle.Puzzle)}. Stopping on [{baseCommand.ExitConditions}] ...");
                    
                    if (pp > 1)
                    {
                        Report.WriteLine("=====================================================================================");
                    }
                    Report.WriteLine("           Name: {0}", puzzle);
                    Report.WriteLine("          Ident: {0}", puzzle.Ident);
                    Report.WriteLine("         Rating: {0}", StaticAnalysis.CalculateRating(puzzle.Puzzle));
                    Report.WriteLine(puzzle.Puzzle.ToString());    // Adds 2x line feeds
                    
                    
                    if (CompSolutions != null && CompSolutions.CheckSkip(puzzle, solver)) 
                    {
                        Progress.WriteLine("Skipping... (SkipPuzzlesWithSolutions)");
                        continue;
                    }

                    // #### Main Block Start --------------------------------------
                    var memStart = GC.GetTotalMemory(false);
                    var attemptTimer = new Stopwatch();
                    attemptTimer.Start();
                    baseCommand.CancellationSource           = new CancellationTokenSource(); // Force a new token for each new run 
                    baseCommand.ExitConditions.ExitRequested = false;
                    state = solver.Init(new SolverCommand(baseCommand, puzzle.Puzzle, baseCommand.ExitConditions)
                    {
                        Report = Report,
                    });
                    var propsReport = GetPropReport(solver, state);
                    Tracking?.Begin(state);
                    
                    try
                    {
                        // ==============[ START SOLVER] ==========================
                        state.Exit = solver.Solve(state);
                    }
                    catch (Exception e)
                    {
                        state.Exception = e;
                        state.Exit      = ExitConditions.Conditions.Error;
                        state.EarlyExit = true;
                    }
                    var memEnd = GC.GetTotalMemory(false);
                    state.Statistics.MemUsed = memEnd;
                    var memDelta = memEnd- memStart;
                    var bytesPerNode = memDelta/state.Statistics.TotalNodes;
                    var maxNodes = (ulong)0;
                    if (DevHelper.TryGetTotalMemory(out var totalMem))
                    {
                        maxNodes = totalMem / (ulong)bytesPerNode;
                    }
                    Report.WriteLine($"Memory Used: {Humanise.SizeSuffix(memEnd)}, delta: {Humanise.SizeSuffix(memDelta)} ~ {bytesPerNode:#,##0} bytes/node => max nodes:{maxNodes:#,##0}");
                    attemptTimer.Stop();
                    // #### Main Block End ------------------------------------------


                    var cleanUp = CodeBlockTimer.Run("Solver finished, wrapping up...", () => {
                        
                        state.Summary = new SolverResultSummary(
                            puzzle,
                            state.Solutions,
                            state.Exit,
                            SolverHelper.GenerateSummary(state),
                            attemptTimer.Elapsed,
                            state.Statistics
                        );

                        res.Add(state.Summary);

                        start.TotalNodes += state.Statistics.TotalNodes;
                        start.TotalDead  += state.Statistics.TotalDead;
                        
                        Report.WriteLine("[DONE] {0}", state.Summary.Text);
                        Progress.WriteLine($" -> {state.Summary.Text}");

                        if (batchArgs != null && batchArgs.Save != null)
                        {
                            SaveStateToFile(batchArgs, state, puzzle);
                        }

                        // Add Depth Reporting
                        GenerateReports(state, solver).Wait();

                        // Building Reports
                        if (CompSolutions != null)
                        {
                            CompSolutions.StoreIfNecessary(state);
                        }


                        if (state?.Summary?.Solutions != null && state.Summary.Solutions.Any()) // May have been removed above
                        {
                            consecutiveFails = 0;
                        }
                        else
                        {
                            consecutiveFails++;
                            if (StopOnConsecutiveFails != 0 && consecutiveFails > StopOnConsecutiveFails)
                            {
                                Progress.WriteLine("ABORTING... StopOnConsecutiveFails");
                                return;
                            }
                        }

                        Tracking?.End(state);

                    });

                    if (cleanUp.Elapsed.TotalSeconds > 10)
                    {
                        Console.WriteLine(cleanUp);    
                    }


                    if (state.Exception != null)
                    {
                        Report.WriteLine("[EXCEPTION]");
                        WriteException(Report, state.Exception);
                    }
                    if (state.Exit == ExitConditions.Conditions.Aborted)
                    {
                        Progress.WriteLine("ABORTING...");
                        if (showSummary) WriteSummary(res, start);
                        return res;
                    }
                    if (start.DurationInSec > run.BatchExit.Duration.TotalSeconds)
                    {
                        Progress.WriteLine("BATCH TIMEOUT...");
                        if (showSummary) WriteSummary(res, start);
                        return res;
                    }

                    Progress.WriteLine();
                }
                catch (Exception ex)
                {
                    if (state != null) state.Exception = ex;
                    Progress.WriteLine("ERROR: " + ex.Message);
                    WriteException(Report, ex);
                }
                finally
                {
                    state = null;
                   
                    if (puzzle != run.Last())
                    {
                        GC.Collect();
                    }
                }

                
            }
            if (showSummary) WriteSummary(res, start);
            
            Report.WriteLine("Completed               : {0}", DateTime.Now.ToString("u"));
            return res;
        }
        private void SaveStateToFile(BatchSolveComponent.BatchArgs batchArgs, SolverState state, LibraryPuzzle puzzle)
        {

            var binSer = new BinaryNodeSerializer();

            var rootForward = state.GetRootForward();
            if (rootForward != null)
            {
                var outState = System.IO.Path.Combine(batchArgs.Save, $"{puzzle.Ident}-forward.ssbn");
                using (var f = File.Create(outState))
                {
                    using (var bw = new BinaryWriter(f))
                    {
                        binSer.WriteTree(bw, rootForward);
                    }
                }
                Report.WriteLine($"\tSaving State: {outState}");
                Progress.WriteLine($"\tSaving State: {outState}");
            }

            var rootReverse = state.GetRootReverse();
            if (rootReverse != null)
            {
                var outState = System.IO.Path.Combine(batchArgs.Save, $"{puzzle.Ident}-reverse.ssbn");
                using (var f = File.Create(outState))
                {
                    using (var bw = new BinaryWriter(f))
                    {
                        binSer.WriteTree(bw, rootReverse);
                    }
                }
                Report.WriteLine($"\tSaving State: {outState}");
                Progress.WriteLine($"\tSaving State: {outState}");
            }
        }

        private async Task GenerateReports(SolverState state, ISolver solver)
        {
            var r = state.Command.Report;
            if (r == null) return;

            if (state.HasSolution)
            {
                foreach (var solution in state.Solutions)
                {
                    r.WriteLine($"SOLUTION: {solution.ToStringSummary()}");
                    r.WriteLine(solution.ToString());
                }    
            }
            

            if (r is TextWriterAdapter ad)
            {
                var renderer = new MapToReportingRendererText();
                var finalStats = solver.Statistics;
                if (finalStats != null)
                {
                    r.WriteLine("### Statistics ###");
                
                    
                    MapToReporting.Create<SolverStatistics>()
                                  .AddColumn("Name", x=>x.Name)
                                  .AddColumn("Nodes", x=>x.TotalNodes)
                                  .AddColumn("Avg. Speed", x=>x.NodesPerSec)
                                  .AddColumn("Duration (sec)", x=>x.DurationInSec)
                                  .AddColumn("Duplicates", x=>x.Duplicates < 0 ? null : (int?)x.Duplicates)
                                  .AddColumn("Warnings", x=>x.Warnings)
                                  .AddColumn("Errors", x=>x.Errors)
                                  .AddColumn("Dead", x=>x.TotalDead < 0 ? null : (int?)x.TotalDead)
                                  .AddColumn("Current Depth", x=>x.DepthCurrent < 0 ? null : (int?)x.DepthCurrent)
                                  .RenderTo(finalStats, renderer, ad.Inner);
                }
                
                var repDepth = MapToReporting.Create<SolverHelper.DepthLineItem>()
                                       .AddColumn("Depth", x => x.Depth)
                                       .AddColumn("Total", x => x.Total)
                                       .AddColumn("Growth Rate", x => x.GrowthRate)
                                       .AddColumn("UnEval", x => x.UnEval)
                                       .AddColumn("Complete", x => (x.Total - x.UnEval) *100 / x.Total, c=>c.ColumnInfo.AsPercentage());
                
                if (state is MultiThreadedSolverState multi)
                {
                    r.WriteLine("### Forward Tree ###");
                    repDepth.RenderTo(await SolverHelper.ReportDepth(multi.Root), renderer, ad.Inner);
                    
                    r.WriteLine("### Reverse Tree ###");
                    repDepth.RenderTo(await SolverHelper.ReportDepth(multi.RootReverse), renderer, ad.Inner);
                }
                else if (state is SingleThreadedSolverState sts)
                {
                    r.WriteLine("### Forward Tree ###");
                    repDepth.RenderTo(await SolverHelper.ReportDepth(sts.Forward?.Root), renderer, ad.Inner);
                    
                    r.WriteLine("### Reverse Tree ###");
                    repDepth.RenderTo(await SolverHelper.ReportDepth(sts.Reverse?.Root), renderer, ad.Inner);
                }
                else if (state is SolverBaseState sb)
                {
                    r.WriteLine("### Forward Tree ###");
                    repDepth.RenderTo(await SolverHelper.ReportDepth(sb.Root), renderer, ad.Inner);
                }
                else
                {
                    // ?
                }
            }
            
            r.WriteLine("======[End Of Report]============================================================================");
            
           

        }

        private FluentString GetPropReport(ISolver solver, SolverState commandState)
        {
            Report.WriteLine("Solver: {0}", SolverHelper.Describe(solver));
            
            var propsReport = new FluentString();
            propsReport.Append(solver.TypeDescriptor);
            try
            {
                var typeDescriptorProps = solver.GetTypeDescriptorProps(commandState);
                if (typeDescriptorProps != null)
                {
                    foreach (var (name, text) in typeDescriptorProps)
                    {
                        propsReport.AppendLine($"-> {name,20}: {text}");
                        Report.WriteLine($"-> {name,20}: {text}");
                    }
                }
            }
            catch (NotSupportedException)
            {
                var msg = $"Solver [{solver.GetType().Name}] does not support {typeof(IExtendedFunctionalityDescriptor).Name}";
                Report.WriteLine(msg);
                propsReport.AppendLine(msg);
            }
            catch (NotImplementedException)
            {
                var msg = $"Solver [{solver.GetType().Name}] does not support {typeof(IExtendedFunctionalityDescriptor).Name}";
                Report.WriteLine(msg);
                propsReport.AppendLine(msg);
            }

            return propsReport;
        }

       


        private void WriteException(ITextWriter report, Exception exception, int indent = 0)
        {
            report.WriteLine("   Type: {0}", exception.GetType().Name);
            report.WriteLine("Message: {0}", exception.Message);
            report.WriteLine(exception.StackTrace);
            if (exception.InnerException != null) WriteException(report, exception.InnerException, indent + 1);
        }

        public void WriteSummary(List<SolverResultSummary> results, SolverStatistics start)
        {
            var cc = 0;
            
            /* Example
           GUYZEN running RT:3.1.3 OS:'WIN 6.2.9200.0' Threads:32 RELEASE x64 'AMD Ryzen Threadripper 2950X 16-Core Processor '
           Git: '[DIRTY] c724b04 Progress notifications, rev:191' at 2020-04-08 09:14:51Z, v3.1.0
            */
            var line = DevHelper.FullDevelopmentContext();
            Report.WriteLine(line);
            if (WriteSummaryToConsole) System.Console.WriteLine(line);
            
            
            foreach (var result in results)
            {
                line = $"[{result.Puzzle.Ident}] {result.Text}";
                Report.WriteLine(line);
                if (WriteSummaryToConsole)System.Console.WriteLine(line);
                cc++;
            }
        }
    }
}