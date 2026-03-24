using System.Runtime.InteropServices;
using SokoSolve.LargeSearchSolver.Lookup;
using SokoSolve.Primitives;

namespace SokoSolve.LargeSearchSolver.MicroBenchmarks;

public static class Scratch
{
    public static int Execute(string[] args)
    {
        MemorySizes();
        // Histogram();

        return 0;
    }

    private static void MemorySizes()
    {
        var before = GC.GetTotalMemory(false);
        var total = 10_000_000;
        var nodes = new NodeHeap(total);
        var ctx = new NSContext(16, 16);
        for(uint cc=0; cc<total; cc++)
        {
            ref var n = ref nodes.GetById(cc);
            n.SetHashCode((int)cc);
            n.SetCrateMapAt(ctx, (byte)(cc % 16), (byte)(cc % 13), true);
            n.SetMoveMapAt(ctx, (byte)(cc % 14), (byte)(cc % 11), true);
        }
        unsafe
        {
            Console.WriteLine($"sizeof(NodeStruct) = {sizeof(NodeStruct)}");
        }
        var after = GC.GetTotalMemory(false);
        Console.WriteLine($"sizeof(NodeStruct[{total}]) = indirect {after - before:#,##0}");

        var tree = new LNodeLookupBlackRedTree(nodes, ctx);
        for(uint cc=0; cc<total; cc++)
        {
            ref var n = ref nodes.GetById(cc);
            tree.Add(ref n);
        }
        var treeSize = GC.GetTotalMemory(false);
        Console.WriteLine($"sizeof(LNodeLookupBlackRedTree[{total}]) = indirect {treeSize - after:#,##0}");

        Console.WriteLine($"Pause on [ENTER] to get process size with PID:{Environment.ProcessId}");
        Console.ReadLine();
        /*
sizeof(NodeStruct) = 84
sizeof(NodeStruct[10000000]) = indirect 840,036,352
sizeof(LNodeLookupBlackRedTree[10000000]) = indirect 879,973,872
Pause on [ENTER] to get process size with PID:210580

Linux 6.18.9-arch1-2 (guyzen-arch)      01/03/26        _x86_64_        (32 CPU)

08:32:07      UID       PID  minflt/s  majflt/s     VSZ     RSS   %MEM  Command
08:32:07     1000    210580      1.28      0.00 135360800 1742276   2.65  SokoSolve.Large
         */
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

