using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SokoSolve.Core.Game;
using SokoSolve.Core.PuzzleLogic;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Debugger;
using SokoSolve.Core.Library;
using SokoSolve.Core.Library.DB;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    public interface ISolverRunTracking
    {
        void Begin(SolverCommandResult command);
        void End(SolverCommandResult result);
    }

    public class SolverRunComponent
    {
        public SolverRunComponent()
        {
            Report = Console.Out;
            Progress = Console.Out;
        }

        /// <summary>
        /// For memory reasons, we cannot allow ANY state from the Solver.
        /// This would cause out of memory issues.
        /// </summary>
        public class Result
        {
            public string Summary { get; set; }
            public ExitConditions.Conditions Exited { get; set; }
            public List<Analytics.Path> Solutions { get; set; }
        }

        public TextWriter Report { get; set; }
        public TextWriter Progress { get; set; }

        // Optional
        public ISokobanRepository Repository { get; set; }

        public ISolverRunTracking Tracking { get; set; }
        public int StopOnConsecutiveFails { get; set; }

        public bool SkipPuzzlesWithSolutions { get; set; }

        public List<Result> Run(SolverRun run, SolverCommand baseCommand, ISolver solver = null)
        {
            if (solver == null)
            {
                solver = new SingleThreadedForwardSolver();
            }
            
            Report.WriteLine("Puzzle Exit Conditions: {0}", run.PuzzleExit);
            Report.WriteLine("Batch Exit Conditions : {0}", run.BatchExit);
            Report.WriteLine("Solver                : {0}", SolverHelper.Describe(solver));
            Report.WriteLine("CPU                   : {0}", DebugHelper.GetCPUDescription());
            Report.WriteLine("Machine               : {0} {1}", Environment.MachineName, Environment.Is64BitProcess ? "x64" : "x32");
            
            Report.WriteLine();

            var res = new List<Result>();
            var start = new SolverStatistics()
            {
                Started = DateTime.Now
            };
            SolverCommandResult result = null;
            int pp = 0;
            int consecutiveFails = 0;
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
                    Progress.WriteLine("Attempting: {0} ({2}/{3}); Rating: {1}", PuzzleHelper.GetName(puzzle), StaticAnalysis.CalculateRating(puzzle), pp, run.Count);
                    

                    var libPuzzle = puzzle as LibraryPuzzle;
                    Report.WriteLine("           Name: {0}", PuzzleHelper.GetName(puzzle));
                    Report.WriteLine("          Ident: {0}", libPuzzle != null ? libPuzzle.Ident : null);
                    Report.WriteLine("         Rating: {0}", StaticAnalysis.CalculateRating(puzzle));
                    Report.WriteLine("Starting Memory: {0}", Environment.WorkingSet);
                    Report.WriteLine(puzzle.ToString());

                    PuzzleDTO dto = null;
                    if (Repository != null)
                    {
                        dto = Repository.Get(puzzle);
                        if (dto == null)
                        {
                            dto = Repository.ToDTO(puzzle);
                            Repository.Store(dto);
                        }
                        dto.Solutions = Repository.GetSolutions(dto.PuzzleId);
                    }

                    if (SkipPuzzlesWithSolutions)
                    {
                        if (dto != null && 
                            dto.Solutions.Exists(x=>x.HostMachine == Environment.MachineName && x.SolverType == solver.GetType().Name))
                        {
                            Progress.WriteLine("Skipping... (SkipPuzzlesWithSolutions)");
                            continue;
                        }
                    }

                    Report.WriteLine();

                    result = solver.Init(new SolverCommand(baseCommand)
                    {
                        Report = Report,
                        ExitConditions = run.PuzzleExit,
                        Puzzle = puzzle,

                    });
                    if (Tracking != null) Tracking.Begin(result);

                    solver.Solve(result);
                    var r = new Result()
                    {
                        Summary = SolverHelper.Summary(result),
                        Exited = result.Exit,
                        Solutions = result.GetSolutions()
                    };
                    res.Add(r);
                    

                    start.TotalNodes += result.Statistics.TotalNodes;
                    start.TotalDead += result.Statistics.TotalDead;


                    if (r.Solutions.Any())
                    {
                        int cc = 0;
                        foreach (var p in r.Solutions.ToArray())
                        {
                            string error = null;
                            var check = SolverHelper.CheckSolution(puzzle, p, out error);
                            Report.WriteLine("Solution #{0} [{1}] =>\n{2}", cc++, check ? "Valid":"INVALID!"+error, p);
                            if (!check)
                            {
                                r.Solutions.Remove(p);
                                r.Summary += " (INVALID SOLUTION)";
                            }
                        }
                        // Write to DB?
                        if (dto != null)
                        {
                            if (Repository != null)
                            {
                                StoreSolution(solver, dto, r.Solutions);
                            }
                        }
                    }

                    if (r.Solutions.Any()) // May have been removed above
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
                        foreach (var fs in finalStats)
                        {
                            Report.WriteLine("Statistics | {0}", fs);
                        }
                    }

                    if (Tracking != null) Tracking.End(result);

                    Report.WriteLine("[DONE] {0}", r.Summary);
                    if (result.Exception != null)
                    {
                        Report.WriteLine("[EXCEPTION]");    
                        WriteException(Report, result.Exception);
                    }
                    

                    Progress.WriteLine();
                    Progress.WriteLine("Completed: {0} ==> {1}", PuzzleHelper.GetName(puzzle), r.Summary);
                    Progress.WriteLine();

                    if (result.Exit == ExitConditions.Conditions.Aborted)
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
                    if (result != null) result.Exception = ex;
                    Progress.WriteLine("ERROR: "+ex.Message);
                    WriteException(Report, ex);
                }
                finally
                {
                    result = null;
                    Report.WriteLine("Ending Memory: {0}", Environment.WorkingSet);
                    GC.Collect();
                    Report.WriteLine("Post-GC Memory: {0}", Environment.WorkingSet);
                    Report.WriteLine("===================================");
                    Report.WriteLine();
                }

                Report.Flush();
            }
            WriteSummary(res, start);
            return res;
        }

        private void StoreSolution(ISolver solver, PuzzleDTO dto, List<Analytics.Path> solutions)
        {
            var best = solutions.OrderBy(x => x.Count).First();

            var sol = new SolutionDTO()
            {
                PuzzleREF = dto.PuzzleId,
                CharPath = best.ToString(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                HostMachine = Environment.MachineName,
                SolverType = solver.GetType().Name,
                SolverVersionMajor = solver.VersionMajor,
                SolverVersionMinor = solver.VersionMinor,
                SolverDescription = solver.VersionDescription,
                TotalNodes = solver.Statistics.First().TotalNodes,
                TotalSecs = (decimal) solver.Statistics.First().DurationInSec,
            };

            var exists = Repository.GetSolutions(dto.PuzzleId);
            if (exists.Any())
            {
                var thisMachine = exists.FindAll(x => x.HostMachine == sol.HostMachine && x.SolverType == sol.SolverType);
                if (thisMachine.Any())
                {
                    var exact = thisMachine.OrderByDescending(x => x.CharPath.Length).First();
                    // Is Better
                    if (exact.TotalSecs > sol.TotalSecs)
                    {
                        // Replace
                        sol.SolutionId = exact.SolutionId;
                        sol.Created = exact.Created;
                        Repository.Update(sol);
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


        private void WriteException(TextWriter report, Exception exception, int indent =0)
        {
            report.WriteLine("   Type: {0}", exception.GetType().Name);
            report.WriteLine("Message: {0}", exception.Message);
            report.WriteLine(exception.StackTrace);
            if (exception.InnerException != null)
            {
                WriteException(report, exception.InnerException, indent+1);
            }
        }

        private void WriteSummary(List<Result> results, SolverStatistics start)
        {
            var cc = 0;
            var line = string.Format("Run Stats: {0}", start);
            Report.WriteLine(line);Console.WriteLine(line);
            foreach (var result in results)
            {
                line = string.Format("[Puzzle] {0}", result.Summary);
                Report.WriteLine(line); Console.WriteLine(line);

                
                cc++;
            }
        }

        
    }
}