using SokoSolve.Primitives;
using Xunit.Abstractions;

namespace SokoSolve.LargeSearchSolver.Tests;

public class FwdRevEvalTests : NodeStructTests
{
    public FwdRevEvalTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public void CanFindChainSolution_Trivial01() => CanFindChainSolution(PuzzleLibraryStatic.Trivial01);

    [Fact]
    public void CanFindChainSolution_Trivial02() => CanFindChainSolution(PuzzleLibraryStatic.Trivial02);

    [Fact]
    public void CanFindChainSolution_Trivial03_NoSolution()
    {
        var request = new LSolverRequest(PuzzleLibraryStatic.Trivial03_NoSolution,
                new() { StopOnSolution = true, MaxNodes = 1000 });

        var coordinator = new SolverCoordinator()
        {
            StateFactory = new SolverCoordinatorFactory
            {
                UnitTest = true,
            }
        };
        var state = coordinator.Init(request);
        var res = coordinator.Solve(state);

        Assert.Equal("Exhaustive", res.Exit);
    }

    void CanFindChainSolution(Puzzle puzzle)
    {
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = true, MaxNodes = 1000 });

        var coordinator = new SolverCoordinator()
        {
            StateFactory = new SolverCoordinatorFactory
            {
                UnitTest = true,
            }
        };
        var state = coordinator.Init(request);
        var res = coordinator.Solve(state);

        Assert.Equal("StopRequested", res.Exit);
        Assert.True(state.HasSolution);
        Assert.Single(state.SolutionsChain);

    }
}
