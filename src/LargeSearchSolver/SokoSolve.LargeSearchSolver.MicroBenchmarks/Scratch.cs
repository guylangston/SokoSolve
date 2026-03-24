using System.Runtime.InteropServices;
using SokoSolve.LargeSearchSolver.Lookup;
using SokoSolve.Primitives;

namespace SokoSolve.LargeSearchSolver.MicroBenchmarks;

public static class Scratch
{
    public static int Execute(string[] args)
    {
        // Histogram();

        return 0;
    }


    private static void Histogram()
    {
        Console.WriteLine("Building Tree... solving...");
        var puzzle = PuzzleLibraryStatic.PQ1_P29;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = false });

        var coordinator = new SolverCoordinator();
        var state = coordinator.Init(request);
        var res = coordinator.Solve(state);

        int[] buckets = new int[16];
        for(uint cc = 0; cc<state.Heap.Count; cc++)
        {
            ref var node = ref state.Heap.GetById(cc);
            var hash = node.HashCode;

            var b = Math.Abs(hash) % buckets.Length;
            buckets[b]++;
        }

        for(int cc=0; cc<buckets.Length; cc++)
        {
            Console.WriteLine($"Bucket {cc,2} -> {buckets[cc]}");
        }

        /* Output
         Bucket  0 -> 68732
Bucket  1 -> 68603
Bucket  2 -> 68112
Bucket  3 -> 69120
Bucket  4 -> 68818
Bucket  5 -> 68733
Bucket  6 -> 68523
Bucket  7 -> 68813
Bucket  8 -> 68690
Bucket  9 -> 68664
Bucket 10 -> 68895
Bucket 11 -> 68632
Bucket 12 -> 68281
Bucket 13 -> 69098
Bucket 14 -> 69156
Bucket 15 -> 68434
        */

    }
}

