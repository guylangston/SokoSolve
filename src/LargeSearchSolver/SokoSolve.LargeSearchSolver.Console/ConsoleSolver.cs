using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using System.Diagnostics;
using VectorInt.Collections;
using SokoSolve.Reporting;
using SokoSolve.LargeSearchSolver.Utils;

namespace SokoSolve.LargeSearchSolver.Console;


public static class ConsoleSolver
{
    public class PuzzleSummary
    {
        public required LibraryPuzzle Puzzle { get; set; }
        public required TimeSpan Time { get; set; }
        public required int TotalNodes { get; set; }
        public required int Solutions { get; set; }
    }

    public class SolverArgs
    {
        public bool WritePid { get; set; }
        public bool GitStatus { get; set; } = true;
        public bool Experimental { get; set; }
        public bool NonInteractiveConsole { get; set; } = true;
        public bool VeryLarge { get; set; } 
    }

    internal static bool StopRun;
    public static async Task<int> Solve(string puzzle, AttemptConstraints constraints, SolverArgs args)
    {
        CReport report = new(System.Console.Out);

        report.WriteLine($"Starting Solver Run... --puzzle {puzzle}");
        if (args.WritePid)
        {
            report.WriteLine($"PID: {Environment.ProcessId} > ./sokosolve.pid");
            File.WriteAllText("sokosolve.pid", Environment.ProcessId.ToString());
        }
        else
        {
            report.WriteLine($"PID: {Environment.ProcessId}");
        }
        report.WriteLine(DevHelper.RuntimeEnvReport());
        if (args.GitStatus)
        {
            report.WriteRaw(outp =>
            {
                WriteGitStatus(outp).Wait();
            });
        }
        foreach(var ln in await OSHelper.GetLinuxMemoryInfo())
        {
            report.WriteLine(ln);
        }
        report.WriteLine(NodeStruct.DescibeMemoryLimits());
        report.WriteLine();

        List<PuzzleSummary> summary = new();
        var solverRun = LoadPuzzles(puzzle, constraints, report, args);
        foreach (var p in solverRun)
        {
            if (StopRun) break;

            // GC here, so that objects below are now out of scope
            GC.Collect();

            // Get memory usage
            var memStart = GC.GetTotalMemory(false);

            report.WriteLine("----------------------------------------------");
            report.WriteLabels(lbl =>
            {
                lbl.Add("Puzzle", p.Name);
                lbl.Add("Ident", p.Ident.ToString());
                lbl.Add("Rating", p.Rating.ToString());
                lbl.Add("Size", p.Puzzle.Size.ToString());
                if (KnownSolutions.TrueSize.FirstOrDefault(x=>x.PuzzleIdent == p.Ident.ToString()) is {} match)
                {
                    if (match.TotalNodesSolution.HasValue)
                        lbl.Add("Known-Size-Solution", match.TotalNodesSolution.Value.ToString("#,##0"));

                    if (match.TotalNodesExhaustive.HasValue)
                        lbl.Add("Known-Size-Exhuastive", match.TotalNodesExhaustive.Value.ToString("#,##0"));

                    if (match.BestAttempt.HasValue)
                        lbl.Add("Best-Attempt", match.BestAttempt.Value.ToString("#,##0"));
                }
            });
            // report.WriteLine($"Puzzle: {p.Name} ({p.Ident}), Rating: {p.Rating}, Size: {p.Puzzle.Size}");
            report.Write(p.Puzzle.ToString());

            if (p.Puzzle.Width > NodeStruct.MaxMapWidth || p.Puzzle.Height > NodeStruct.MaxMapHeight)
            {
                report.WriteLine("     SKIPPING. Puzzle too large.");
                continue;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var lPuzzle = ConvertPuzzle(p.Puzzle);
            var request = new LSolverRequest(lPuzzle, constraints);
            var coordinator = new SolverCoordinator()
            {
                Peek = args.NonInteractiveConsole ? null : new SolverCoodinatorPeekConsole()
            };
            if (coordinator.StateFactory is SolverCoordinatorFactory sf)
            {
                if (args.Experimental)
                {
                    sf.Experimental = args.Experimental;
                    report.WriteLine("Flags: EXPERIMENTAL");
                }
                if (args.VeryLarge)
                {
                    sf.VeryLarge = args.VeryLarge;
                    report.WriteLine("Flags: VERYLARGE");
                }
            }
            var state = coordinator.Init(request);
            report.WriteLabels(l =>
            {
                foreach (var item in coordinator.DescribeComponents(state))
                {
                    l.Add($"CMP {item.Name}", item.Desc);
                }
            });
            var res = coordinator.Solve(state);
            var realHeap = (NodeHeap)state.Heap;

            stopWatch.Stop();
            var memEnd = GC.GetTotalMemory(false);
            System.Console.WriteLine(); // Clear progress bar

            report.WriteLabels(l =>
            {
                var nodesPerSec = res.StatusTotalNodesEvaluated / stopWatch.Elapsed.TotalSeconds;
                var sol = state.Solutions.Count > 0 ? $"SOLUTION!({state.Solutions.Count})" : "FAILED";

                l.Add("Completed", stopWatch.ToString());
                l.Add("Memory used", $"{(memEnd - memStart) / 1024 / 1024}MB");
                l.Add("Total nodes", $"{res.StatusTotalNodesEvaluated:#,##0} at {nodesPerSec:#,##0.0}nodes/sec");
                l.Add("Result", sol);
            });

            summary.Add(new PuzzleSummary()
            {
                Puzzle = p,
                Time = stopWatch.Elapsed,
                TotalNodes = res.StatusTotalNodesEvaluated,
                Solutions = state.Solutions.Count,
            });
        }

        // Write `summary` as a ASCII table
        report.WriteLine("Summary:");
        report.WriteTable(tbl=>
        {
            tbl.AddLine("Puzzle", "Rating", "Time(sec)", "Nodes", "Solutions", "Machine", "Version");
            foreach (var s in summary)
            {
                tbl.AddLine(s.Puzzle.Ident, s.Puzzle.Rating, s.Time.TotalSeconds.ToString("0.0"),  s.TotalNodes,s.Solutions, 
                        Environment.MachineName, SolverCoordinator.SolverVersion);
            }
        });
        if (args.WritePid)
        {
            File.Delete("sokosolve.pid");
        }
        return 0;
    }

    private static SolverRun LoadPuzzles(string puzzle, AttemptConstraints constraints, CReport report, SolverArgs args)
    {
        var pathHelper = new PathHelper();
        var compLib = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));
        var repSol = new JsonSokobanSolutionRepository(pathHelper.GetRelDataPath("Lib/solutions.json"));
        if (puzzle.StartsWith("__"))
        {
            var selection = compLib.GetPuzzlesWithCachingUsingRegex("SQ1");
            if (!selection.Any())
            {
                throw new Exception($"No puzzles found '{puzzle}'");
            }
            report.WriteLine($"Puzzles With KnownSeachSize: {KnownSolutions.TrueSize.Count}");
            var solverRun = new SolverRun();
            solverRun.Init();
            if (puzzle == "__known")
            {
                solverRun.AddRange(KnownSolutions.TrueSize.Select(x=>selection.First(y=>y.Ident.ToString() == x.PuzzleIdent)));
                return solverRun;
            }
            if (puzzle == "__target")
            {
                solverRun.AddRange(KnownSolutions.NextTargets.Select(x=>selection.First(y=>y.Ident.ToString() == x)));
                return solverRun;
            }
            throw new Exception(puzzle);
        }
        else
        {
            var selection = compLib.GetPuzzlesWithCachingUsingRegex(puzzle);
            if (!selection.Any())
            {
                throw new Exception($"No puzzles found '{puzzle}'");
            }
            report.WriteLine($"Available Puzzles: {selection.Count()}");
            var solverRun = new SolverRun();
            solverRun.Init();
            solverRun.AddRange( selection .Where(Filter) .OrderBy(x => x.Rating));
            return solverRun;
        }

        bool Filter(LibraryPuzzle x)
        {
            return     (constraints.MinRating == null || x.Rating >= constraints.MinRating)
                    && (constraints.MaxRating == null || x.Rating <= constraints.MaxRating);
        }
    }

    private static Primitives.Puzzle ConvertPuzzle(Puzzle puzzle)
    {
        var lp = Primitives.Puzzle.Builder.FromLines(puzzle.ToStringList());
        return lp;
    }

    // See: /home/guy/repo/Rustlings/src/GL.Helpers/ProcessHelper.cs
    public static async Task<List<string>> RunYieldingStdOutAsList(string prog, string args, string? directory = null)
    {
        using var proc = new Process
        {
            StartInfo = new ProcessStartInfo(prog, args)
            {
               WorkingDirectory       = directory,
               RedirectStandardOutput = true,
               UseShellExecute        = false,
            }
        };
        proc.Start();
        var res = new List<string>();
        while(await proc.StandardOutput.ReadLineAsync() is {} line)
        {
            res.Add(line);
        }
        return res;
    }

    private static async Task WriteGitStatus(TextWriter outp)
    {
        if (File.Exists("/usr/bin/git"))
        {
            await WriteGitStatus("/usr/bin/git", outp);
        }
        else
        {
            if (OSHelper.TryFindInEnvironmentPath(OSHelper.IsWindows() ? "git.exe" : "git", out var gitPath))
            {
                await WriteGitStatus(gitPath, outp);
            }
            else
            {
                outp.WriteLine("git not found");
            }
        }

        async Task WriteGitStatus(string bin, TextWriter outp)
        {
            foreach(var ln in await RunYieldingStdOutAsList(bin, "log -1"))
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                await outp.WriteLineAsync($"GIT-LOG1: {ln}");
            }
            foreach(var ln in await RunYieldingStdOutAsList(bin, "status"))
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                if (ln.TrimStart().StartsWith('(')) continue;
                await outp.WriteLineAsync($"GIT-STAT: {ln}");
            }
        }

    }
}



