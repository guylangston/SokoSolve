using BenchmarkDotNet.Running;
using ObjectLayoutInspector;

namespace SokoSolve.LargeSearchSolver.MicroBenchmarks;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            var summary = BenchmarkRunner.Run<FloodFillBenchmark>();
            return 0;
        }

        if (args[0] == "--profile")
        {
            Console.WriteLine("Running bench directly for profiling...");
            var test = new MemoryUsageBenchmark();
            test.Standard();
            return 0;
        }

        if (args[0] == "--layout")
        {
            ObjectLayoutInspector.TypeLayout.PrintLayout<NodeStruct>();
        }

        return 1;
    }
}
