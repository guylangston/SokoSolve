using SokoSolve.Primitives;

namespace SokoSolve.LargeSearchSolver.Tests;

public class FwdRevEvalTests
{
    [Fact]
    public void CanFindChainSolution()
    {
        var puzzle = PuzzleLibraryStatic.Trivial02;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = true, MaxNodes = 100 });

        var coordinator = new SolverCoordinator()
        {
            StateFactory = new SolverCoordinatorFactory
            {
                UnitTest = true,
            }
        };
        var state = coordinator.Init(request);
        var res = coordinator.Solve(state);

        Assert.True(state.HasSolution);
        Assert.Single(state.SolutionsChain);

    }
}
