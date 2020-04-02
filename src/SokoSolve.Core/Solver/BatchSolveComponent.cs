using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    public interface ISolverRunTracking
    {
        void Begin(SolverCommandResult command);
        void End(SolverCommandResult result);
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
        public List<Path>                Solutions  { get; }
        public ExitConditions.Conditions Exited     { get;  }
        public string                    Text       { get;  }
        public TimeSpan                  Duration   { get;  }
        public SolverStatistics          Statistics { get;  }
    }

    public class BatchSolveComponent
    {
        public BatchSolveComponent(TextWriter report, TextWriter progress, ISokobanSolutionRepository? repository, ISolverRunTracking? tracking, int stopOnConsecutiveFails, bool skipPuzzlesWithSolutions)
        {
            Report = report;
            Progress = progress;
            Repository = repository;
            Tracking = tracking;
            StopOnConsecutiveFails = stopOnConsecutiveFails;
            SkipPuzzlesWithSolutions = skipPuzzlesWithSolutions;
        }

        public BatchSolveComponent(TextWriter report, TextWriter progress)
        {
            Report = report;
            Progress = progress;
            Repository = null;
            Tracking = null;
            StopOnConsecutiveFails = 5;
            SkipPuzzlesWithSolutions = false;
        }

        public TextWriter          Report                   { get; }
        public TextWriter          Progress                 { get; }
        public ISokobanSolutionRepository? Repository               { get; }
        public ISolverRunTracking? Tracking                 { get; }
        public int                 StopOnConsecutiveFails   { get; }
        public bool                SkipPuzzlesWithSolutions { get; }

        public List<SolverResultSummary> Run(SolverRun run, SolverCommand baseCommand, ISolver solver)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (baseCommand == null) throw new ArgumentNullException(nameof(baseCommand));
            if (solver == null)
                throw new ArgumentNullException(nameof(solver), "See: " + nameof(SingleThreadedForwardSolver));

            Report.WriteLine("Puzzle Exit Conditions: {0}", run.PuzzleExit);
            Report.WriteLine("Batch Exit Conditions : {0}", run.BatchExit);
            Report.WriteLine("Environment           : {0}", DevHelper.RuntimeEnvReport());
            Report.WriteLine("Solver                : {0}", SolverHelper.Describe(solver));
            Report.WriteLine("Started               : {0}", DateTime.Now.ToString("u"));
            Report.WriteLine();

            var res = new List<SolverResultSummary>();
            var start = new SolverStatistics
            {
                Started = DateTime.Now
            };
            SolverCommandResult? commandResult = null;
            var pp = 0;
            var consecutiveFails = 0;
            foreach (var puzzle in run)
            {
                if (baseCommand.CheckAbort(baseCommand))
                {
                    Progress.WriteLine("EXITING...");
                    break;
                }

                try
                {
                    pp++;
                    Progress.WriteLine($"({pp}/{run.Count}) Attempting: {puzzle.Ident} \"{puzzle.Name}\", R={StaticAnalysis.CalculateRating(puzzle.Puzzle)}. Stopping:[{baseCommand.ExitConditions}] ...");

                    
                    Report.WriteLine("           Name: {0}", puzzle);
                    Report.WriteLine("          Ident: {0}", puzzle.Ident);
                    Report.WriteLine("         Rating: {0}", StaticAnalysis.CalculateRating(puzzle.Puzzle));
                    Report.WriteLine(puzzle.Puzzle.ToString());
                    Report.WriteLine();


                    IReadOnlyCollection<SolutionDTO> existingSolutions = null;
                    if (SkipPuzzlesWithSolutions && Repository != null) 
                    {
                        existingSolutions =  Repository.GetPuzzleSolutions(puzzle.Ident);
                        if (existingSolutions != null && existingSolutions.Any(
                            x => x.MachineName == Environment.MachineName && x.SolverType == solver.GetType().Name))
                        {
                            Progress.WriteLine("Skipping... (SkipPuzzlesWithSolutions)");
                            continue;    
                        }
                    }

                    Report.WriteLine();
                    
                    // #### Main Block Start
                    var attemptTimer = new Stopwatch();
                    attemptTimer.Start();
                    commandResult = solver.Init(new SolverCommand(baseCommand)
                    {
                        Report = Report,
                        Puzzle = puzzle.Puzzle
                    });
                    Tracking?.Begin(commandResult);
                    solver.Solve(commandResult);
                    attemptTimer.Stop();
                    // #### Main Block End
                    
                    
                    if (Repository != null)
                    {
                        StoreAttempt(solver, puzzle, commandResult);
                    }

                    commandResult.Summary = new SolverResultSummary(
                        puzzle,
                        commandResult.Solutions,
                        commandResult.Exit,
                        SolverHelper.GenerateSummary(commandResult),
                        attemptTimer.Elapsed,
                        commandResult.Statistics
                    );

                    res.Add(commandResult.Summary);

                    start.TotalNodes += commandResult.Statistics.TotalNodes;
                    start.TotalDead  += commandResult.Statistics.TotalDead;

                    if (commandResult.Summary.Solutions.Any()) // May have been removed above
                    {
                        consecutiveFails = 0;
                    }
                    else
                    {
                        consecutiveFails++;
                        if (StopOnConsecutiveFails != 0 && consecutiveFails > StopOnConsecutiveFails)
                        {
                            Progress.WriteLine("ABORTING... StopOnConsecutiveFails");
                            break;
                        }
                    }

                    var finalStats = solver.Statistics;
                    if (finalStats != null)
                    {
                        Report.WriteLine("Statistics:");
                        foreach (var fs in finalStats)
                            Report.WriteLine(" -> {0}", fs);
                    }
                        

                   
                    if (Tracking != null) Tracking.End(commandResult);

                    Report.WriteLine("[DONE] {0}", commandResult.Summary.Text);
                    if (commandResult.Exception != null)
                    {
                        Report.WriteLine("[EXCEPTION]");
                        WriteException(Report, commandResult.Exception);
                    }

                    Progress.WriteLine($" -> {commandResult.Summary.Text}");

                    if (commandResult.Exit == ExitConditions.Conditions.Aborted)
                    {
                        Progress.WriteLine("ABORTING...");
                        WriteSummary(res, start);
                        return res;
                    }

                    if (start.DurationInSec > run.BatchExit.Duration.TotalSeconds)
                    {
                        Progress.WriteLine("BATCH TIMEOUT...");
                        WriteSummary(res, start);
                        return res;
                    }

                    Progress.WriteLine();
                }
                catch (Exception ex)
                {
                    if (commandResult != null) commandResult.Exception = ex;
                    Progress.WriteLine("ERROR: " + ex.Message);
                    WriteException(Report, ex);
                }
                finally
                {
                    commandResult = null;
                    Report.WriteLine("Ending Memory: {0}", Environment.WorkingSet);
                    GC.Collect();
                    Report.WriteLine("Post-GC Memory: {0}", Environment.WorkingSet);
                    Report.WriteLine("===================================");
                    Report.WriteLine();
                }

                Report.Flush();
            }

            WriteSummary(res, start);
            
            Report.WriteLine("Completed               : {0}", DateTime.Now.ToString("u"));
            return res;
        }

        private void StoreAttempt(ISolver solver, LibraryPuzzle dto, SolverCommandResult result)
        {
            var best = result.Solutions?.OrderBy(x => x.Count).FirstOrDefault();

            var sol = new SolutionDTO
            {
                IsAutomated        =  true,
                PuzzleIdent        = dto.Ident.ToString(),
                Path               = best?.ToString(),
                Created            = DateTime.Now,
                Modified           = DateTime.Now,
                MachineName        = Environment.MachineName,
                MachineCPU =        DevHelper.DescribeCPU(),
                SolverType         = solver.GetType().Name,
                SolverVersionMajor = solver.VersionMajor,
                SolverVersionMinor = solver.VersionMinor,
                SolverDescription  = solver.VersionDescription,
                TotalNodes         = solver.Statistics.First().TotalNodes,
                TotalSecs          = solver.Statistics.First().DurationInSec
            };

            var exists = Repository.GetPuzzleSolutions(dto.Ident);
            if (exists != null && exists.Any())
            {
                var onePerMachine= exists.FirstOrDefault(x => x.MachineName == sol.MachineName && x.SolverType == sol.SolverType);
                if (onePerMachine != null)
                {
                    if (sol.HasSolution )
                    {
                        if (!onePerMachine.HasSolution)
                        {
                            sol.SolutionId = onePerMachine.SolutionId; // replace
                            Repository.Store(sol);    
                        }
                        else if (sol.TotalSecs < onePerMachine.TotalSecs)
                        {
                            sol.SolutionId = onePerMachine.SolutionId; // replace
                            Repository.Store(sol);
                        }
                        else
                        {
                            // drop
                        }
                        
                    }
                    else 
                    {
                        if (!onePerMachine.HasSolution && sol.TotalNodes < onePerMachine.TotalNodes)
                        {
                            sol.SolutionId = onePerMachine.SolutionId; // replace
                            Repository.Store(sol);
                        }
                    }
                }
                else
                {
                    Repository.Store(sol);
                }
            }
            else
            {
                Repository.Store(sol);
            }
        }


        private void WriteException(TextWriter report, Exception exception, int indent = 0)
        {
            report.WriteLine("   Type: {0}", exception.GetType().Name);
            report.WriteLine("Message: {0}", exception.Message);
            report.WriteLine(exception.StackTrace);
            if (exception.InnerException != null) WriteException(report, exception.InnerException, indent + 1);
        }

        private void WriteSummary(List<SolverResultSummary> results, SolverStatistics start)
        {
            var cc = 0;
            var line = DevHelper.RuntimeEnvReport();
            Report.WriteLine(line);
            Console.WriteLine(line);
            foreach (var result in results)
            {
                line = $"[{result.Puzzle.Ident}] {result.Text}";
                Report.WriteLine(line);
                Console.WriteLine(line);
                cc++;
            }
        }

        
    }
}