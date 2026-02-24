using BenchmarkDotNet.Attributes;
using SokoSolve.Core.Lib;
using SokoSolve.LargeSearchSolver.Lookup;

namespace SokoSolve.LargeSearchSolver.MicroBenchmarks;

[MemoryDiagnoser]
public class MemoryUsageBenchmark
{
    LibraryPuzzle Puzzle = SokoSolve.Core.Lib.TestLibrary.Default;

    [Benchmark]
    public void StandardPuzzleSmall()
    {
        var request = new LSolverRequest(Puzzle.Puzzle, new AttemptConstraints { StopOnSolution = false });
        var coordinator = new SolverCoordinator();
        var state = coordinator.Init(request);
        var res = coordinator.Solve(state);
    }

    [Benchmark]
    public void StandardPuzzleSmall_CompoundLookup()
    {
        var puzzle = SokoSolve.Core.Lib.TestLibrary.Default;
        var request = new LSolverRequest(puzzle.Puzzle, new AttemptConstraints { StopOnSolution = false });
        var coordinator = new SolverCoordinator();
        var state = coordinator.Init(request);
        state.Heap = new NodeHeap((int)KnownSolutions.TrueSize.First(x=>x.PuzzleIdent==puzzle.Ident.ToString()).TotalNodesSolution + 1);
        state.Lookup = new LNodeLookupCompound(state.Heap)
        {
            ThresholdDynamic = 100, // to make comparisons to larger size more realistic
        };
        var res = coordinator.Solve(state);
    }

    [Benchmark(Baseline = true)]
    public void StandardPuzzleSmall_NaieveLookup()
    {
        var puzzle = SokoSolve.Core.Lib.TestLibrary.Default;
        var request = new LSolverRequest(puzzle.Puzzle, new AttemptConstraints { StopOnSolution = false });
        var coordinator = new SolverCoordinator();
        var state = coordinator.Init(request);
        state.Heap = new NodeHeap((int)KnownSolutions.TrueSize.First(x=>x.PuzzleIdent==puzzle.Ident.ToString()).TotalNodesSolution + 1);
        state.Lookup = new LNodeLookupLinkedList(state.Heap);
        var res = coordinator.Solve(state);
    }
}

