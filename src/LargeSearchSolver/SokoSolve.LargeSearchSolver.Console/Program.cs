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
            Console.WriteLine($"Completed: {stopWatch}");
            Console.WriteLine();
        }

        return 0;
    }
}


