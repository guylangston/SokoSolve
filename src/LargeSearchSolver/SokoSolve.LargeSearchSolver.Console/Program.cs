using SokoSolve.LargeSearchSolver;
using SokoSolve.Core;
using SokoSolve.Core.Lib;

namespace SokoSolve.LargeSearchSolver.Console;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var puzzle = TestLibrary.Default;
        var request = new LSolverRequest(puzzle.Puzzle);

        var coordinator = new SolverCoordinator();
        var state = coordinator.Init(request);

        var res = await coordinator.Solve(state, new CancellationToken());

        var realHeap = (NodeHeap)state.Heap;

        return 0;
    }
}


