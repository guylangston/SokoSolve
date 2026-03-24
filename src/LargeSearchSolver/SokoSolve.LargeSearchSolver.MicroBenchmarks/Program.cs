using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using System.CommandLine;

namespace SokoSolve.LargeSearchSolver.MicroBenchmarks;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        RootCommand root = new("SokoSolve LargeSearchSolver MicroBenchmarks");

        Command micro = new("micro", "Run micro benchmarks and profiling tools");

        Command bitmap = new("bitmap", "Run bitmap benchmark");
        bitmap.SetAction(_ =>
        {
            var config = DefaultConfig.Instance
            .AddExporter(MarkdownExporter.GitHub);

            var summary = BenchmarkRunner.Run([ typeof(BitmapBenchmark_Read), typeof(BitmapBenchmark_Write) ], config);
        });

        Command floodfill = new("floodfill", "Run floodfill benchmark");
        floodfill.SetAction(_ =>
        {
            BenchmarkRunner.Run<FloodFillBenchmark>();
        });

        Command profile = new("profile", "Run benchmark directly for profiling");
        profile.SetAction(_ =>
        {
            Console.WriteLine("Running bench directly for profiling...");
            var test = new MemoryUsageBenchmark();
            test.Standard();
        });

        Command layout = new("layout", "Print memory layout of NodeStruct");
        layout.SetAction(_ =>
        {
            ObjectLayoutInspector.TypeLayout.PrintLayout<NodeStruct>();
        });

        Command scratch = new("scratch", "Run once-off random experiments");
        scratch.SetAction(_ =>
        {
            Scratch.Execute(Array.Empty<string>());
        });

        micro.Subcommands.Add(bitmap);
        micro.Subcommands.Add(floodfill);
        micro.Subcommands.Add(profile);
        micro.Subcommands.Add(layout);
        root.Subcommands.Add(micro);

        root.Subcommands.Add(scratch);
        return await root.Parse(args).InvokeAsync();
    }
}
