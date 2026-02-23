using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using System.Diagnostics;
using VectorInt.Collections;

namespace SokoSolve.LargeSearchSolver.ConsoleHost;

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
    }

    internal static bool StopRun;
    public static async Task<int> Solve(string puzzle, AttemptConstraints constraints, SolverArgs args)
    {
        Console.WriteLine($"Starting Solver Run... --puzzle {puzzle}");
        if (args.WritePid)
        {
            Console.WriteLine($"PID: {Environment.ProcessId} > ./sokosolve.pid");
            File.WriteAllText("sokosolve.pid", Environment.ProcessId.ToString());
        }
        else
        {
            Console.WriteLine($"PID: {Environment.ProcessId}");
        }
        Console.WriteLine(DevHelper.RuntimeEnvReport());
        Console.WriteLine(NodeStruct.DescibeMemoryLimits());
        Console.WriteLine();

        var pathHelper = new PathHelper();
        var compLib    = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));
        var repSol     = new JsonSokobanSolutionRepository(pathHelper.GetRelDataPath("Lib/solutions.json"));

        var selection = compLib.GetPuzzlesWithCachingUsingRegex(puzzle);
        if (!selection.Any())
        {
            throw new Exception($"No puzzles found '{puzzle}'");
        }
        Console.WriteLine($"Available Puzzles: {selection.Count()}");
        var solverRun = new SolverRun();
        solverRun.Init();
        solverRun.AddRange(
            selection
                .Where(x =>
                    (constraints.MinRating == null || x.Rating >= constraints.MinRating)
                    && (constraints.MaxRating == null || x.Rating <= constraints.MaxRating))
                .OrderBy(x=>KnownSolutions.TrueSize.FirstOrDefault(x=>x.PuzzleIdent == x.PuzzleIdent)?.TotalNodesSolution ?? uint.MaxValue)
                .ThenBy(x=>x.Rating)
                );

        List<PuzzleSummary> summary = new();

        foreach(var p in solverRun)
        {
            if (StopRun) break;

            // GC here, so that objects below are now out of scope
            GC.Collect();

            // Get memory usage
            var memStart = GC.GetTotalMemory(false);

            Console.WriteLine("----------------------------------------------");
            Console.WriteLine($"Puzzle: {p.Name} ({p.Ident}), Rating: {p.Rating}, Size: {p.Puzzle.Size}");
            Console.Write(p.Puzzle);

            if (p.Puzzle.Width > NodeStruct.MaxMapWidth || p.Puzzle.Height > NodeStruct.MaxMapHeight)
            {
                Console.WriteLine("     SKIPPING. Puzzle too large.");
                continue;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var request = new LSolverRequest(p.Puzzle, constraints);
            var coordinator = new SolverCoordinator()
            {
                Peek = new SolverCoodinatorPeekConsole()
            };
            var state = coordinator.Init(request);
            var res = await coordinator.Solve(state, new CancellationToken());
            var realHeap = (NodeHeap)state.Heap;

            stopWatch.Stop();
            var memEnd = GC.GetTotalMemory(false);
            Console.WriteLine(); // Clear progress bar
            Console.WriteLine($"Completed: {stopWatch}");
            Console.WriteLine($"Memory used: {(memEnd - memStart)/1024/1024}MB");
            var nodesPerSec = res.StatusTotalNodesEvaluated / stopWatch.Elapsed.TotalSeconds;
            Console.WriteLine($"Total Nodes: {res.StatusTotalNodesEvaluated:#,##0} at {nodesPerSec:#,##0.0}nodes/sec");
            var sol = state.Solutions.Count > 0 ? $"SOLUTION!({state.Solutions.Count})"  : "FAILED";
            Console.WriteLine($"Result: {sol}");
            Console.WriteLine();

            summary.Add( new PuzzleSummary()
            {
                Puzzle = p,
                Time = stopWatch.Elapsed,
                TotalNodes = res.StatusTotalNodesEvaluated,
                Solutions = state.Solutions.Count,
            });
        }

        // Write `summary` as a ASCII table
        Console.WriteLine("Summary:");
        Console.WriteLine($"{"Puzzle",-10} {"Rating",6} {"Time",10} {"Nodes",15} Solutions");
        Console.WriteLine(new string('-', 65));
        foreach(var s in summary)
        {
            Console.WriteLine($"{s.Puzzle.Ident,-10} {s.Puzzle.Rating,6} {s.Time.TotalSeconds,10} {s.TotalNodes,15:#,##0}, {s.Solutions}");
        }
        if (args.WritePid)
        {
           File.Delete("sokosolve.pid");
        }
        return 0;
    }
}



