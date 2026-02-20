using SokoSolve.LargeSearchSolver;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using System.Diagnostics;

namespace SokoSolve.LargeSearchSolver.ConsoleHost;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await Mayy();

        var puzzle = TestLibrary.Default;
        var request = new LSolverRequest(puzzle.Puzzle);

        var coordinator = new SolverCoordinator();
        var state = coordinator.Init(request);

        var res = await coordinator.Solve(state, new CancellationToken());

        var realHeap = (NodeHeap)state.Heap;

        return 0;
    }

    public static async Task<int> Mayy()
    {
        var pathHelper = new PathHelper();
        var compLib    = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));
        var repSol     = new JsonSokobanSolutionRepository(pathHelper.GetRelDataPath("Lib/solutions.json"));

        var puzzleSearchRegEx = "SQ1";
        var selection = compLib.GetPuzzlesWithCachingUsingRegex(puzzleSearchRegEx);
        if (!selection.Any())
        {
            throw new Exception($"No puzzles found '{puzzleSearchRegEx}'");
        }

        var solverRun = new SolverRun();
        solverRun.Init();
        solverRun.AddRange(
                selection
                .OrderBy(x=>x.Rating)
                .Where(x=>x.Rating >= 0 && x.Rating <= 50)
                );

        List<(LibraryPuzzle, TimeSpan, int)> summary = new();

        foreach(var p in solverRun)
        {
            Console.WriteLine($"Puzzle: {p.Name}, Rating: {p.Rating}");
            Console.Write(p.Puzzle);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var request = new LSolverRequest(p.Puzzle);
            var coordinator = new SolverCoordinator();
            var state = coordinator.Init(request);
            var res = await coordinator.Solve(state, new CancellationToken());
            var realHeap = (NodeHeap)state.Heap;

            stopWatch.Stop();
            Console.WriteLine($"Total Nodes: {res.StatusTotalNodesEvaluated}");
            Console.WriteLine($"Solutions Ids: {string.Join(',', state.Solutions)}");
            Console.WriteLine($"Completed: {stopWatch}");
            Console.WriteLine();

            summary.Add( (p, stopWatch.Elapsed, res.StatusTotalNodesEvaluated) );

            GC.Collect();
        }

        foreach(var s in summary)
        {
            Console.WriteLine($"{s.Item1.Name,30} | {s.Item2.TotalSeconds.ToString("0.0"),-20} | {s.Item3,10}");
        }

        return 0;
    }
}


