using System.Text;
using SokoSolve.Primitives;
using SokoSolve.Reporting;

namespace SokoSolve.LargeSearchSolver.Tests;

/// <summary>Helper class to quickly setup solver state, components, etc</summary>
public static class SolverInit
{
    public static LSolverState Setup_UnitTest(Puzzle puzzle, string[] tags)
    {
        var req = new LSolverRequest(puzzle, new AttemptConstraints()
        {
            // Setup some safe default for quick tests
            StopOnSolution = true,
            MaxNodes = 1000_000,
            MaxTime = 10,
        });
        var coord = new SolverCoordinator()
        {
            StateFactory = new SolverCoordinatorFactory(tags)
            {
                UnitTest = true,
            }
        };
        var state = coord.Init(req);
        return state;
    }

    public static string DescribeComponents(LSolverState state)
    {
        var sb = new StringBuilder();
        if (state.Coordinator is SolverCoordinator sc)
        {
            using (var tw = new StringWriter(sb))
            {
                CReport report = new(tw);
                report.WriteLabels(l =>
                {
                    l.Add($"CMP NodeStruct", NodeStruct.Describe());
                    foreach (var item in sc.DescribeComponents(state))
                    {
                        l.Add($"CMP {item.Name}", item.Desc);
                    }
                });
            }
        }

        return sb.ToString();
    }
}





