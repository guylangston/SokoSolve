using System;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using SokoSolve.Game;
using SokoSolve.Game.Scenes;

namespace SokoSolve.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var verb = args.Length == 0 ? "default" : args.FirstOrDefault();
            
            System.Console.WriteLine("====================================================");
            System.Console.WriteLine($"{Application.Name} - v{Application.Version}");
            System.Console.WriteLine("====================================================");
            System.Console.WriteLine(SolverHelper.DescribeCPU()); 
            System.Console.WriteLine(SolverHelper.DescribeHostMachine());
            System.Console.WriteLine();

            if (string.Equals(verb, "batch", StringComparison.InvariantCultureIgnoreCase))
            {
                RunBatchSolve(args.Length > 1 ? args[1] : "SQ1");
            }
            else if (string.Equals(verb, "benchmark", StringComparison.InvariantCultureIgnoreCase))
            {
                RunProfile(args.Contains("-long"), args.Length > 1 ? args[1] : "SQ1~P5");
            }
            else if (string.Equals(verb, "play", StringComparison.InvariantCultureIgnoreCase) ||
                     string.Equals(verb, "default", StringComparison.InvariantCultureIgnoreCase))
            {
                RunPlay();
            }
            else if (string.Equals(verb, "microBench", StringComparison.InvariantCultureIgnoreCase))
            {
                //var summary = BenchmarkRunner.Run<BaseLineSolvers>();
                //var summary = BenchmarkRunner.Run<SolverNodeLookupBenchmark>();
            }
            else if (string.Equals(verb, "adhoc", StringComparison.InvariantCultureIgnoreCase))
            {
                // var x = new SolverNodeLookupBenchmark();
                // x.Setup();
                // System.Console.WriteLine("Setup complete....");
                //
                // var timer = new Stopwatch();
                // timer.Start();
                // x.SolverNodeLookupThreadSafeBuffer_Multi();
                // timer.Stop();
                // System.Console.WriteLine($"{x.GetType().Namespace}::SolverNodeLookupThreadSafeBuffer_Multi - {timer.Elapsed}");
                //
                // if (false)
                // {
                //     GC.Collect();
                //
                //     timer = new Stopwatch();
                //     timer.Start();
                //     x.SolverNodeLookupByBucketWrap_Multi();
                //     timer.Stop();
                //     System.Console.WriteLine($"{x.GetType().Namespace}::SolverNodeLookupByBucketWrap_Multi - {timer.Elapsed}");    
                // }
                
            }
        }

        private static void RunProfile(bool longRun, string ident)
        {
            var pathHelper = new PathHelper();
            var compLib = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));

            var solverRun = new SolverRun();
            
            var puzzle = compLib.GetPuzzleWithCaching(PuzzleIdent.Parse(ident));
            solverRun.Init();
            solverRun.Add(puzzle);
            
            var exitRequested = false;
            var solverCommand = new SolverCommand
            {
                ExitConditions = longRun 
                        ? new ExitConditions() { Duration =  TimeSpan.FromMinutes(20), StopOnSolution = true }
                        : ExitConditions.Default3Min(),
                CheckAbort = x => exitRequested,
                //Progress = new ConsoleProgressNotifier()
            };

            var outFile = $"./profile--{DateTime.Now:s}.txt".Replace(':', '-');
            System.Console.WriteLine($"See ./{outFile} for a more detailed report.");

            using var report = File.CreateText(outFile);
            System.Console.CancelKeyPress += (o, e) =>
            {
                report.Flush();
                System.Console.WriteLine("Ctrl+C detected; cancel requested");

                solverCommand.ExitConditions.ExitRequested = true;
                exitRequested = true;
            };

            var runner = new BatchSolveComponent(report, System.Console.Out);
            
            runner.Run(solverRun, solverCommand, new MultiThreadedForwardReverseSolver());   
            
        }

        private static void RunPlay()
        {
            if (true)
            {
                System.Console.CursorVisible  = false;
                System.Console.OutputEncoding = Encoding.Unicode;

                var scale     = 1.5;
                var charScale = 3;
            
                // Setup: Display
                //DirectConsole.MaximizeWindow();
                var cons = DirectConsole.Setup(
                    (int)(80 * scale),  
                    (int)(25 * scale), 
                    7*charScale, 
                    10*charScale, 
                    "Consolas");
            
                var renderer = new ConsoleRendererCHAR_INFO(cons);
                var bridge   = new BridgeSokobanPixelToCHAR_INFO(renderer);
            
                // Setup: Input 
                var input = new InputProvider()
                {
                    IsMouseEnabled = true
                };

                using(var consoleLoop = new ConsoleGameLoop<SokobanPixel>(input, bridge))
                {
                    using(var master = new SokoSolveMasterGameLoop(consoleLoop))
                    {
                        consoleLoop.Scene = master;
                        consoleLoop.Init();
                        consoleLoop.Start();    
                    }    
                }    
            }
            else
            {
                System.Console.SetBufferSize(120, 60);
                System.Console.SetWindowSize(120, 60);
                var bridge   = new BridgeSokobanPixelToConsolePixel(new ConsolePixelRenderer(BasicDirectConsole.Singleton));
            
                // Setup: Input 
                var input = new InputProvider()
                {
                    IsMouseEnabled = true
                };

                using(var consoleLoop = new ConsoleGameLoop<SokobanPixel>(input, bridge))
                {
                    consoleLoop.SetGoalFramesPerSecond(5);
                    using(var master = new SokoSolveMasterGameLoop(consoleLoop))
                    {
                        consoleLoop.Scene = master;
                        consoleLoop.Init();
                        consoleLoop.Start();    
                    }    
                }    
            }
            
        }

        private static void RunBatchSolve(string libIdent)
        {
            var pathHelper = new PathHelper();
            var lib = new LibraryComponent(pathHelper.GetDataPath());

            var solverRun = new SolverRun();
            solverRun.Load(lib.GetLibraryWithCaching(libIdent)
                              .Where(x=>x.Rating > 100 && x.Rating < 1500));

            var exitRequested = false;
            var solverCommand = new SolverCommand
            {
                ExitConditions = ExitConditions.OneMinute(),
                CheckAbort = x => exitRequested
            };

            System.Console.WriteLine("See ./solver.txt for a more detailed report.");
            using var report = File.CreateText("results/solver.txt");
            System.Console.CancelKeyPress += (o, e) =>
            {
                report.Flush();
                System.Console.WriteLine("Ctrl+C detected; cancel requested");

                solverCommand.ExitConditions.ExitRequested = true;
                exitRequested = true;
            };
            
            var repoSol = new JsonSokobanSolutionRepository("solutions.json");

            var runner = new BatchSolveComponent(
                report,
                System.Console.Out,
                repoSol,
                null,
                5,
                false);
                
            runner.Run(solverRun, solverCommand, new MultiThreadedForwardReverseSolver());
        }
    }
}
