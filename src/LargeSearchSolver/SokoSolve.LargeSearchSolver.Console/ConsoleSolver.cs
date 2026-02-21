using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using System.Diagnostics;

namespace SokoSolve.LargeSearchSolver.ConsoleHost;

public static class ConsoleSolver
{
    internal static bool StopRun;
    public static async Task<int> Solve(string puzzle, AttemptConstraints constraints)
    {
        Console.WriteLine($"Starting Solver Run... --puzzle {puzzle}");
        Console.WriteLine($"{Environment.MachineName} PID:{Environment.ProcessId}");
        Console.WriteLine(DevHelper.RuntimeEnvReport());
        unsafe
        {
            var memNodes = OSHelper.GetAvailableMemory();
            Console.WriteLine($"sizeof({nameof(NodeStruct)})={sizeof(NodeStruct)}. TheorticalNodeLimit={memNodes/sizeof(NodeStruct):#,##0}");
        }
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
                .OrderBy(x=>x.Rating));

        List<(LibraryPuzzle, TimeSpan, int)> summary = new();

        foreach(var p in solverRun)
        {
            if (StopRun) break;

            // GC here, so that object below are now out of scope
            GC.Collect();

            // Get memory usage
            var memStart = GC.GetTotalMemory(false);

            Console.WriteLine("----------------------------------------------");
            Console.WriteLine($"Puzzle: {p.Name} ({p.Ident}), Rating: {p.Rating}");
            Console.Write(p.Puzzle);
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
            Console.WriteLine($"Memory used: {memEnd - memStart}");
            var nodesPerSec = res.StatusTotalNodesEvaluated / stopWatch.Elapsed.TotalSeconds;
            Console.WriteLine($"Total Nodes: {res.StatusTotalNodesEvaluated:#,##0} at {nodesPerSec:#,##0.0}nodes/sec");
            var sol = state.Solutions.Count > 0 ? $"SOLUTION!({state.Solutions.Count})"  : "FAILED";
            Console.WriteLine($"Result: {sol}");
            Console.WriteLine();

            summary.Add( (p, stopWatch.Elapsed, res.StatusTotalNodesEvaluated) );

        }

        foreach(var s in summary)
        {
            Console.WriteLine($"{s.Item1.Ident,10} | {s.Item1.Rating,4} | {s.Item2.TotalSeconds.ToString("0.0"),-10} | {s.Item3,10}");
        }

        return 0;
    }
}



