using SokoSolve.LargeSearchSolver;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using System.CommandLine;
using System.Diagnostics;

namespace SokoSolve.LargeSearchSolver.ConsoleHost;

public static class Program
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
        Option<int> maxNodes = new("--maxNodes", "-m")
        {
            Description = "Max Nodes then abort",
            Required = false,
        };
        Option<int> maxDepth = new("--maxDepth", "-d")
        {
            Description = "Max Depth then abort",
            Required = false,
        };
        Option<int> maxTimeSecs = new("--maxTime", "-t")
        {
            Description = "Max Time (seconds) then abort",
            Required = false,
        };
        Option<float> minRating = new("--minRating", "-R")
        {
            Description = "Min Puzzle Rating to attempt",
            Required = false,
        };
        Option<float> maxRating = new("--maxRating", "-r")
        {
            Description = "Max Puzzle Rating to attempt",
            Required = false,
        };
        solve.Add(puzzle);
        solve.Add(maxNodes);
        solve.Add(maxDepth);
        solve.Add(minRating);
        solve.Add(maxRating);
        solve.Add(maxTimeSecs);
        root.Subcommands.Add(solve);

        solve.SetAction(pr=>
                {
                    var constraints = new AttemptConstraints()
                    {
                        MaxNodes = pr.GetValue(maxNodes),
                        MaxDepth = pr.GetValue(maxDepth),
                        MaxTime = pr.GetValue(maxTimeSecs),
                        MinRating = pr.GetValue(minRating),
                        MaxRating = pr.GetValue(maxRating)
                    };
                    var task = Solve(pr.GetValue(puzzle) ?? "SQ1~P5", constraints);
                    task.Wait();
                });

        return await root.Parse(args).InvokeAsync();
    }

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
                .Where(x=> constraints.MinRating == null || x.Rating >= constraints.MinRating)
                .Where(x=> constraints.MaxRating == null || x.Rating <= constraints.MaxRating)
                .OrderBy(x=>x.Rating));

        List<(LibraryPuzzle, TimeSpan, int)> summary = new();

        foreach(var p in solverRun)
        {
            if (StopRun) break;
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
            Console.WriteLine(); // Clear progress bar
            Console.WriteLine($"Completed: {stopWatch}");
            var nodesPerSec = res.StatusTotalNodesEvaluated / stopWatch.Elapsed.TotalSeconds;
            Console.WriteLine($"Total Nodes: {res.StatusTotalNodesEvaluated:#,##0} at {nodesPerSec:#,##0.0}nodes/sec");
            var sol = state.Solutions.Count > 0 ? $"SOLUTION!({state.Solutions.Count})"  : "FAILED";
            Console.WriteLine($"Result: {sol}");
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


