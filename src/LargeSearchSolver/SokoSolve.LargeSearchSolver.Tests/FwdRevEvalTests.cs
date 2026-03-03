using SokoSolve.Primitives;

namespace SokoSolve.LargeSearchSolver.Tests;

public class FwdRevEvalTests
{
    [Fact]
    public void CanFindChainSolution()
    {
        var puzzle = PuzzleLibraryStatic.Trivial02;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = false });

        var coordinator = new SolverCoordinator()
        {
        };
        var state = coordinator.Init(request);
        state.EvalReverse = new LNodeStructEvaluatorReverse();
        state.EvalForward = new LNodeStructEvaluatorForwardStable();
        state.HashCalculator = new NodeHashCalculator();
        var res = coordinator.Solve(state);

    }
}
