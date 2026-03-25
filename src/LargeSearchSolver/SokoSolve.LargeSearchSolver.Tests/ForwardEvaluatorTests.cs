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
            | .PMMM. | 
            | .MMCM. | FWD
            | .MCMM. | COMPLETE
            | .MMMM. | dX:0, dY:0
            | ...... |
            | ...... | NodeId:1 -> ParentId:0
            | .MMMM. | 
            | .MMPM. | FWD
            | .MCCM. | COMPLETE
            | .MMMM. | dX:0, dY:1
            | ...... |
            | ...... | NodeId:2 -> ParentId:0
            | .MMMM. | 
            | .MCPM. | FWD
            | .MCMM. | COMPLETE
            | .MMMM. | dX:-1, dY:0
            | ...... |
            | ...... | NodeId:3 -> ParentId:0
            | .MMMM. | 
            | .MMCM. | FWD
            | .MPCM. | COMPLETE
            | .MMMM. | dX:1, dY:0
            | ...... |
            | ...... | NodeId:4 -> ParentId:0
            | .MMCM. | 
            | .MMPM. | FWD
            | .MCMM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:5 -> ParentId:0
            | .MMMM. | 
            | .MCCM. | FWD
            | .MPMM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:6 -> ParentId:1
            | .MMMM. | 
            | .MCMM. | FWD
            | .MPCM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:7 -> ParentId:4
            | .MCPM. | 
            | .MMMM. | FWD
            | .MCMM. | COMPLETE
            | .MMMM. | dX:-1, dY:0
            | ...... |
            | ...... | NodeId:8 -> ParentId:4
            | .MMCM. | 
            | .MMMM. | FWD
            | .MPCM. | COMPLETE
            | .MMMM. | dX:1, dY:0
            | ...... |
            | ...... | NodeId:9 -> ParentId:4
            | .MMCM. | 
            | .MCMM. | FWD
            | .MPMM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:10 -> ParentId:5
            | .MCMM. | 
            | .MPCM. | FWD
            | .MMMM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:11 -> ParentId:6
            | .MCMM. | 
            | .MPMM. | FWD
            | .MMCM. | SOLUTION
            | .MMMM. | dX:0, dY:-1
            | ...... | Solution(Fwd)
            | ...... | NodeId:12 -> ParentId:7
            | .MCMM. | 
            | .MCMM. | FWD
            | .MPMM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:13 -> ParentId:8
            | .MMCM. | 
            | .MMCM. | FWD
            | .MMPM. | COMPLETE
            | .MMMM. | dX:0, dY:-1
            | ...... |
            | ...... | NodeId:14 -> ParentId:9
            | .MCCM. | 
            | .MPMM. | FWD
            | .MMMM. | COMPLETE_LEAF
            | .MMMM. | dX:0, dY:-1
            | ...... |

            """;

        AssertNodeReportEqual(expect, state, state.Heap.EnumerateNodeIds);

    }
}



