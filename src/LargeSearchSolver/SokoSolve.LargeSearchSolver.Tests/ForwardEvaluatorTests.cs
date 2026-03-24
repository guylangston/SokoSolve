using SokoSolve.Primitives;
using Xunit.Abstractions;

namespace SokoSolve.LargeSearchSolver.Tests;

public class ForwardEvaluatorTests : NodeStructTestBase
{
    public ForwardEvaluatorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public void CanEvalAllNodes()
    {
        var puzzle = PuzzleLibraryStatic.Trivial01;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = false });

        var coordinator = new SolverCoordinator()
        {
            StateFactory = new SolverCoordinatorFactory()
            {
                UnitTest = true,
                Tags = new HashSet<string>([ "FwdOnly", "FwdStable", "-DEAD" ]) // don't check dynamic dead
            },
        };
        var state = coordinator.Init(request);
        var res = coordinator.Solve(state);

        Assert.Equal(14, res.StatusTotalNodesEvaluated);
        Assert.Single(state.SolutionsForward);

        var expect =
            """
            | ...... | NodeId:0 -> ParentId:(null)
            | .PMMM. | #164439409 stable
            | .MMCM. | FWD
            | .MCMM. | COMPLETE
            | .MMMM. | dX:0, dY:0
            | ...... |
            | ...... | NodeId:1 -> ParentId:0
            | .MMMM. | #2133565809 stable
            | .MMPM. | FWD
            | .MCCM. | COMPLETE
            | .MMMM. | dX:0, dY:1
            | ...... |
            | ...... | NodeId:2 -> ParentId:0
            | .MMMM. | #1038180089 stable
            | .MCPM. | FWD
            | .MCMM. | COMPLETE
            | .MMMM. | dX:-1, dY:0
            | ...... |
            | ...... | NodeId:3 -> ParentId:0
            | .MMMM. | #275261929 stable
            | .MMCM. | FWD
            | .MPCM. | COMPLETE
            | .MMMM. | dX:1, dY:0
            | ...... |
            | ...... | NodeId:4 -> ParentId:0
            | .MMCM. | #1914546545 stable
            | .MMPM. | FWD
            | .MCMM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:5 -> ParentId:0
            | .MMMM. | #-820123791 stable
            | .MCCM. | FWD
            | .MPMM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:6 -> ParentId:1
            | .MMMM. | #1149002609 stable
            | .MCMM. | FWD
            | .MPCM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:7 -> ParentId:4
            | .MCPM. | #-234249991 stable
            | .MMMM. | FWD
            | .MCMM. | COMPLETE
            | .MMMM. | dX:-1, dY:0
            | ...... |
            | ...... | NodeId:8 -> ParentId:4
            | .MMCM. | #2025369065 stable
            | .MMMM. | FWD
            | .MPCM. | COMPLETE
            | .MMMM. | dX:1, dY:0
            | ...... |
            | ...... | NodeId:9 -> ParentId:4
            | .MMCM. | #929983345 stable
            | .MCMM. | FWD
            | .MPMM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:10 -> ParentId:5
            | .MCMM. | #-2092553871 stable
            | .MPCM. | FWD
            | .MMMM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:11 -> ParentId:6
            | .MCMM. | #-123427471 stable
            | .MPMM. | FWD
            | .MMCM. | SOLUTION
            | .MMMM. | dX:0, dY:-1
            | ...... | Solution(Fwd)
            | ...... | NodeId:12 -> ParentId:7
            | .MCMM. | #-1218813191 stable
            | .MCMM. | FWD
            | .MPMM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:13 -> ParentId:8
            | .MMCM. | #56242665 stable
            | .MMCM. | FWD
            | .MMPM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:14 -> ParentId:9
            | .MCCM. | #-342446735 stable
            | .MPMM. | FWD
            | .MMMM. | COMPLETE_LEAF
            | .MMMM. | dX:0, dY:-1
            | ...... |

            """;

        AssertNodeReportEqual(expect, state, state.Heap.EnumerateNodeIds);

    }
}



