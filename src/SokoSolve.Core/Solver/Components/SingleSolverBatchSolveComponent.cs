using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
        public SolverResultSummary(LibraryPuzzle puzzle, List<Path> solutions, ExitResult exited, 
            string text, TimeSpan duration, SolverStatistics statistics)
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
        public ExitResult                Exited      { get;  }
        public string                    Text       { get;  }
        public TimeSpan                  Duration   { get;  }
        public SolverStatistics          Statistics { get;  }
    }

    public class SingleSolverBatchSolveComponent
    {
        public SingleSolverBatchSolveComponent(
            ITextWriter report, ITextWriter progress, ISokobanSolutionComponent? compSolutions, 
            ISolverRunTracking? tracking, int stopOnConsecutiveFails, bool skipPuzzlesWithSolutions)
        {
            Report = report;
            Progress = progress;
            CompSolutions = compSolutions;
            Tracking = tracking;
            StopOnConsecutiveFails = stopOnConsecutiveFails;
            SkipPuzzlesWithSolutions = skipPuzzlesWithSolutions;
        }

        public ITextWriter                Report                   { get; }
        public ITextWriter                Progress                 { get; }
        public ISokobanSolutionComponent? CompSolutions            { get; }
        public ISolverRunTracking?        Tracking                 { get; }
        public int                        StopOnConsecutiveFails   { get; }
        public bool                       SkipPuzzlesWithSolutions { get; }
        public bool                       WriteSummaryToConsole    { get; set; } = true;

        public List<SolverResultSummary> SolveOneSolverManyPuzzles(
            SolverRun run,
            bool showSummary,
            SolverBuilder builder,
            Dictionary<string, string> solverArgs)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));

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
            
            var pp = 0;
            var consecutiveFails = 0;
            foreach (var puzzle in run)
            {
                if (consecutiveFails >= StopOnConsecutiveFails)
                {
                    Progress.WriteLine($"EXITING... Consecutive Fails{consecutiveFails}");
                    break;
                }

                try
                {
                    pp++;
                    
                    // Build Command & State
                    var attemptArgs = new Dictionary<string, string>(solverArgs);
                    attemptArgs["puzzle"] = puzzle.Ident.ToString();
                    
                    // Build Command => Solver => State
                    SolverState state      = builder.BuildFrom(puzzle, attemptArgs,
                        enrichCommand => 
                        {
                            enrichCommand.Report      = Report;
                            enrichCommand.Progress    = null; // TODO
                            enrichCommand.AggProgress = new ConsoleProgressNotifier(new TextWriterAdapter(TextWriter.Null));  // Bit of a Hack. Multicast to CSV file and Screen
                        },
                        enrichState => { });
                    
                    
                    Progress.WriteLine($"(Puzzle   {pp}/{run.Count}) Attempting: {puzzle.Ident} \"{puzzle.Name}\", " +
                                       $"R={StaticAnalysis.CalculateRating(puzzle.Puzzle)}. " +
                                       $"Stopping on [{state.Command.ExitConditions}] ...");
                    
                    if (pp > 1)
                    {
                        Report.WriteLine("=====================================================================================");
                    }
                    Report.WriteLine("           Name: {0}", puzzle.Name);
                    Report.WriteLine("          Ident: {0}", puzzle.Ident);
                    Report.WriteLine("         Rating: {0}", StaticAnalysis.CalculateRating(puzzle.Puzzle));
                    Report.WriteLine(puzzle.Puzzle.ToString());    // Adds 2x line feeds
                    
                    if (CompSolutions != null && CompSolutions.CheckSkip(puzzle, state.Solver)) 
                    {
                        Progress.WriteLine("Skipping... (SkipPuzzlesWithSolutions)");
                        continue;
                    }

                    // #### Main Block Start --------------------------------------
                    var memStart = GC.GetTotalMemory(false);
                    var attemptTimer = new Stopwatch();
                    
                    try
                    {
                        attemptTimer.Start();

                        var propsReport = GetPropReport(state);
                        Tracking?.Begin(state);
                        
                        // ==============[ START SOLVER] ==========================
                        state.Exit = state.Solver.Solve(state);
                    }
                    catch (Exception e)
                    {
                        state.Exception = e;
                        state.Exit      = ExitResult.Error;
                        state.EarlyExit = true;
                    }
                    var memEnd = GC.GetTotalMemory(false);
                    state.GlobalStats.MemUsed = memEnd;
                    var memDelta = memEnd- memStart;
                    var bytesPerNode = memDelta/state.GlobalStats.TotalNodes;
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
                            state.GlobalStats
                        );

                        res.Add(state.Summary);

                        start.TotalNodes += state.GlobalStats.TotalNodes;
                        start.TotalDead  += state.GlobalStats.TotalDead;
                        
                        Report.WriteLine("[DONE] {0}", state.Summary.Text);
                        Progress.WriteLine($" -> {state.Summary.Text}");

                        if (solverArgs.TryGetValue("save", out var saveFile))
                        {
                            SaveStateToFile(state, puzzle, saveFile);
                        }

                        // Add Depth Reporting
                        GenerateReports(state).Wait();

                        // Building Reports
                        if (CompSolutions != null)
                        {
                            CompSolutions.StoreIfNecessary(state);
                        }


                        if (state.Summary?.Solutions != null && state.Summary.Solutions.Any()) // May have been removed above
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
                        Report.WriteLine(cleanUp.ToString());    
                    }


                    if (state.Exception != null)
                    {
                        Report.WriteLine("[EXCEPTION]");
                        WriteException(Report, state.Exception);
                    }
                    if (state.Exit == ExitResult.Aborted)
                    {
                        Progress.WriteLine("ABORTING...");
                        if (showSummary) WriteSummary(res, start);
                        return res;
                    }
                    if (start.DurationInSec > run.BatchExit?.Duration.TotalSeconds)
                    {
                        Progress.WriteLine("BATCH TIMEOUT...");
                        if (showSummary) WriteSummary(res, start);
                        return res;
                    }

                    Progress.WriteLine();
                }
                catch (Exception ex)
                {
                    Progress.WriteLine("ERROR: " + ex.Message);
                    WriteException(Report, ex);
                }
                finally
                {
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
        
        
        private void SaveStateToFile(SolverState state, LibraryPuzzle puzzle, string path)
        {
            var binSer = new BinaryNodeSerializer();

            var (fwd, rev) = SolverStateHelper.GetTreeState(state);
            
            if (fwd != null)
            {
                var outState = System.IO.Path.Combine(path, $"{puzzle.Ident}-forward.ssbn");
                using (var f = File.Create(outState))
                {
                    using (var bw = new BinaryWriter(f))
                    {
                        binSer.WriteTree(bw, fwd.Root);
                    }
                }
                Report.WriteLine($"\tSaving State: {outState}");
                Progress.WriteLine($"\tSaving State: {outState}");
            }

            
            if (rev != null)
            {
                var outState = System.IO.Path.Combine(path, $"{puzzle.Ident}-reverse.ssbn");
                using (var f = File.Create(outState))
                {
                    using (var bw = new BinaryWriter(f))
                    {
                        binSer.WriteTree(bw, rev.Root);
                    }
                }
                Report.WriteLine($"\tSaving State: {outState}");
                Progress.WriteLine($"\tSaving State: {outState}");
            }
        }

        private async Task GenerateReports(SolverState state)
        {
            var r = state.Command.Report;
            if (r == null) return;
            
            r.WriteLine("======[Report]============================================================================");

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
                
                r.WriteLine("### Type Descriptors ###");
                if (state.GetTypeDescriptors() != null)
                {
                    foreach (var descriptor in state.GetTypeDescriptors())
                    {
                        if (descriptor == null) continue;
                        
                        r.WriteLine($"{descriptor.TypeDescriptor} ({descriptor.GetType().Name})");
                        if (descriptor.GetTypeDescriptorProps(state) != null)
                        {
                            foreach (var prop in descriptor.GetTypeDescriptorProps(state))      
                            {
                                r.WriteLine($"  -> {prop.name}: {prop.text}");   
                            }
                        }
                        
                        
                    }
                }
                else
                {
                    r.WriteLine("Not Supported");
                }
                r.WriteLine("");


                var finalStats = state.Statistics;
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


                var (fwd, rev) = SolverStateHelper.GetTreeState(state);
                if (fwd != null)
                {
                    r.WriteLine("### Forward Tree ###");
                    repDepth.RenderTo(await SolverHelper.ReportDepth(fwd.Root), renderer, ad.Inner);
                }
                if (rev != null)
                {
                    r.WriteLine("### Reverse Tree ###");
                    repDepth.RenderTo(await SolverHelper.ReportDepth(rev.Root), renderer, ad.Inner);
                }
            }
            
            r.WriteLine("======[End Of Report]============================================================================");
            
           

        }

        private FluentString GetPropReport(SolverState state)
        {
            Report.WriteLine("Solver: {0}", SolverHelper.Describe(state.Solver));
            
            var propsReport = new FluentString();
            propsReport.Append(state.Solver.TypeDescriptor);
            try
            {
                var typeDescriptorProps = state.Solver.GetTypeDescriptorProps(state);
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
                var msg = $"Solver [{state.Solver.GetType().Name}] does not support {typeof(IExtendedFunctionalityDescriptor).Name}";
                Report.WriteLine(msg);
                propsReport.AppendLine(msg);
            }
            catch (NotImplementedException)
            {
                var msg = $"Solver [{state.Solver.GetType().Name}] does not support {typeof(IExtendedFunctionalityDescriptor).Name}";
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
            if (WriteSummaryToConsole) Progress.WriteLine(line);
            
            
            foreach (var result in results)
            {
                line = $"[{result.Puzzle.Ident}] {result.Text}";
                Report.WriteLine(line);
                if (WriteSummaryToConsole) Progress.WriteLine(line);
                cc++;
            }
        }
    }
}