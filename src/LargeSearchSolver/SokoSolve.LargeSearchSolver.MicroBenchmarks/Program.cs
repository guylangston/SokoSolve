using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SokoSolve.LargeSearchSolver.Lookup;

namespace SokoSolve.LargeSearchSolver.MicroBenchmarks;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            var summary = BenchmarkRunner.Run<MemoryUsageBenchmark>();
            return 0;
        }

        if (args[0] == "--profile")
        {
            Console.WriteLine("Running bench directly for profiling...");
            var test = new MemoryUsageBenchmark();
            test.StandardPuzzleSmall();
            return 0;
        }

        return 1;
    }
}

[MemoryDiagnoser]
public class MemoryUsageBenchmark
{
    [Benchmark]
    public void StandardPuzzleSmall()
    {
        var puzzle = SokoSolve.Core.Lib.TestLibrary.Default;
        var request = new LSolverRequest(puzzle.Puzzle, new AttemptConstraints { StopOnSolution = false });
        var coordinator = new SolverCoordinator();
        var state = coordinator.Init(request);
        var res = coordinator.Solve(state, new CancellationToken());
        res.Wait();
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
        var res = coordinator.Solve(state, new CancellationToken());
        res.Wait();
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
        var res = coordinator.Solve(state, new CancellationToken());
        res.Wait();
    }
}
