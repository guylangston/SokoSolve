using SokoSolve.LargeSearchSolver;
using System.CommandLine;

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
                    var task = ConsoleSolver.Solve(pr.GetValue(puzzle) ?? "SQ1~P5", constraints);
                    task.Wait();
                });

        return await root.Parse(args).InvokeAsync();
    }

}


