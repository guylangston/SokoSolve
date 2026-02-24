using BenchmarkDotNet.Attributes;
using SokoSolve.Core.Lib;

namespace SokoSolve.LargeSearchSolver.MicroBenchmarks;

[MemoryDiagnoser]
public class MemoryUsageBenchmark
{
    // TODO: Multiple puzzles via
    // https://benchmarkdotnet.org/articles/features/parameterization.html
    LibraryPuzzle Puzzle = SokoSolve.Core.Lib.TestLibrary.Default;

    [Benchmark(Baseline = true)]
    public void Standard_BaseLine()
    {
        var request = new LSolverRequest(Puzzle.Puzzle, new AttemptConstraints { StopOnSolution = false });
        var coordinator = new SolverCoordinator();
        if (coordinator.StateFactory is SolverCoordinatorFactory fs)
        {
            fs.BaseLine = true;
        }

        var state = coordinator.Init(request);
        var res = coordinator.Solve(state);
    }

    [Benchmark]
    public void Standard()
    {
        var request = new LSolverRequest(Puzzle.Puzzle, new AttemptConstraints { StopOnSolution = false });
        var coordinator = new SolverCoordinator();
        var state = coordinator.Init(request);
        var res = coordinator.Solve(state);
    }

    [Benchmark]
    public void Standard_MemorySaving()
    {
        var request = new LSolverRequest(Puzzle.Puzzle, new AttemptConstraints { StopOnSolution = false });
        var coordinator = new SolverCoordinator();
        if (coordinator.StateFactory is SolverCoordinatorFactory fs)
        {
            fs.MemorySaving = true;
        }

        var state = coordinator.Init(request);
        var res = coordinator.Solve(state);
    }

    [Benchmark]
    public void Standard_Experimental()
    {
        var request = new LSolverRequest(Puzzle.Puzzle, new AttemptConstraints { StopOnSolution = false });
        var coordinator = new SolverCoordinator();
        if (coordinator.StateFactory is SolverCoordinatorFactory fs)
        {
            fs.Experimental = true;
        }

        var state = coordinator.Init(request);
        var res = coordinator.Solve(state);
    }
}

