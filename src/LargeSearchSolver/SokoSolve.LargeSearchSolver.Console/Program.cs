using SokoSolve.LargeSearchSolver;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using System.CommandLine;
using System.Diagnostics;

namespace SokoSolve.LargeSearchSolver.ConsoleHost;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        RootCommand root = new("SoloSole LargeSearchSolver");

        Command solve = new("solve", "Solve Puzzle");
        Option<string> puzzle = new("--puzzle", "-p")
        {
            Description = "Example SQ1~P5",
            Required = true,
        };
        solve.Add(puzzle);
        root.Subcommands.Add(solve);

        solve.SetAction(pr=>
                {
                    Solve(pr.GetValue(puzzle), null, null, null).Wait();
                });

        return await root.Parse(args).InvokeAsync();
    }

    public static async Task<int> Solve(string puzzle, int? maxNodes, int? maxTime, int ?maxDepth)
    {
        Console.WriteLine($"Staring Solver Run... --puzzle {puzzle}");

        var pathHelper = new PathHelper();
        var compLib    = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));
        var repSol     = new JsonSokobanSolutionRepository(pathHelper.GetRelDataPath("Lib/solutions.json"));

        var selection = compLib.GetPuzzlesWithCachingUsingRegex(puzzle);
        if (!selection.Any())
        {
            throw new Exception($"No puzzles found '{puzzle}'");
        }

        var solverRun = new SolverRun();
        solverRun.Init();
        solverRun.AddRange( selection .OrderBy(x=>x.Rating));

        List<(LibraryPuzzle, TimeSpan, int)> summary = new();

        foreach(var p in solverRun)
        {
            Console.WriteLine($"Puzzle: {p.Name} ({p.Ident}), Rating: {p.Rating}");
            Console.WriteLine($"{Environment.MachineName} PID:{Environment.ProcessId}");
            Console.WriteLine(DevHelper.FullDevelopmentContext());
            Console.Write(p.Puzzle);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var request = new LSolverRequest(p.Puzzle);
            var coordinator = new SolverCoordinator();
            var state = coordinator.Init(request);
            var res = await coordinator.Solve(state, new CancellationToken());
            var realHeap = (NodeHeap)state.Heap;

            stopWatch.Stop();
            Console.WriteLine($"Total Nodes: {res.StatusTotalNodesEvaluated:#,##0}");
            Console.WriteLine($"Solutions Ids: {string.Join(',', state.Solutions)}");
            Console.WriteLine($"Completed: {stopWatch}");
            Console.WriteLine();

            summary.Add( (p, stopWatch.Elapsed, res.StatusTotalNodesEvaluated) );

            GC.Collect();
        }

        foreach(var s in summary)
        {
            Console.WriteLine($"{s.Item1.Ident,10} | {s.Item1.Rating,4} | {s.Item2.TotalSeconds.ToString("0.0"),-10} | {s.Item3,10}");
        }

        return 0;
    }
}


