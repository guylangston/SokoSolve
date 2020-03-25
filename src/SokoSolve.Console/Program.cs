using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Running;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Console.Benchmarks;
using SokoSolve.Core;
using SokoSolve.Core.Game;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using SokoSolve.Core.Game.Scenes;
using SokoSolve.Tests.Benchmarks;

namespace SokoSolve.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var verb = args.Length == 0 ? "default" : args.FirstOrDefault();
            
            System.Console.WriteLine("====================================================");
            System.Console.WriteLine($"{Host.Name} - v{Host.Version}");
            System.Console.WriteLine("====================================================");
            System.Console.WriteLine();

            if (verb == "Batch")
            {
                RunSolve();
            }
            else if (verb == "Profile" )
            {
                RunProfile(args.Contains("-long"));
            }
            else if (verb == "Play" || verb == "default")
            {
                RunPlay();
            }
            else if (verb == "Bench")
            {
                //var summary = BenchmarkRunner.Run<BaseLineSolvers>();
                var summary = BenchmarkRunner.Run<SolverNodeLookupBenchmark>();
            }
            else if (verb == "BenchTest")
            {
                var x = new SolverNodeLookupBenchmark();
                x.Setup();
                System.Console.WriteLine("Setup complete....");
                
                var timer = new Stopwatch();
                timer.Start();
                x.SolverNodeLookupThreadSafeBuffer_Multi();
                timer.Stop();
                System.Console.WriteLine($"{x.GetType().Namespace}::SolverNodeLookupThreadSafeBuffer_Multi - {timer.Elapsed}");

                if (false)
                {
                    GC.Collect();
                
                    timer = new Stopwatch();
                    timer.Start();
                    x.SolverNodeLookupByBucketWrap_Multi();
                    timer.Stop();
                    System.Console.WriteLine($"{x.GetType().Namespace}::SolverNodeLookupByBucketWrap_Multi - {timer.Elapsed}");    
                }
                
            }
        }

        private static void RunProfile(bool longRun)
        { 
            var libName = "Lib\\SokoSolve-v1\\Sasquatch.ssx";
            
            var pathHelper = new PathHelper();
            var compLib = new LibraryComponent(pathHelper.GetDataPath());

            var solverRun = new SolverRun();
            
            var lib = compLib.LoadLibrary(compLib.GetPathData(libName));
            solverRun.Init();
            solverRun.Add(lib.Find(x=>x.Name == "Grim Town"));
            
            var exitRequested = false;
            var solverCommand = new SolverCommand
            {
                ExitConditions = longRun 
                        ? new ExitConditions() { Duration =  TimeSpan.FromHours(1), StopOnSolution = true }
                        : ExitConditions.Default3Min(),
                CheckAbort = x => exitRequested
            };

            var outFile = $"profile--{DateTime.Now:s}.txt".Replace(':', '-');
            System.Console.WriteLine($"See ./{outFile} for a more detailed report.");

            
            using var report = File.CreateText(outFile);
            System.Console.CancelKeyPress += (o, e) =>
            {
                report.Flush();
                System.Console.WriteLine("Ctrl+C detected; cancel requested");

                solverCommand.ExitConditions.ExitRequested = true;
                exitRequested = true;
            };

            var runner = new SolverRunComponent
            {
                Progress = System.Console.Out,
                Report = report
            };
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

        private static void RunSolve()
        {
            var libName = "Lib\\SokoSolve-v1\\Microban.ssx";
            //var libName = "Lib\\SokoSolve-v1\\Sasquatch.ssx";


            var pathHelper = new PathHelper();
            var lib = new LibraryComponent(pathHelper.GetDataPath());

            var solverRun = new SolverRun();
            solverRun.Load(lib.LoadLibrary(lib.GetPathData(libName)));

            var exitRequested = false;
            var solverCommand = new SolverCommand
            {
                ExitConditions = ExitConditions.OneMinute(),
                CheckAbort = x => exitRequested
            };

            System.Console.WriteLine("See ./solver.txt for a more detailed report.");
            using var report = File.CreateText("solver.txt");
            System.Console.CancelKeyPress += (o, e) =>
            {
                report.Flush();
                System.Console.WriteLine("Ctrl+C detected; cancel requested");

                solverCommand.ExitConditions.ExitRequested = true;
                exitRequested = true;
            };

            var runner = new SolverRunComponent
            {
                Progress = System.Console.Out,
                Report = report
            };
            runner.Run(solverRun, solverCommand, new MultiThreadedForwardReverseSolver());
        }
    }
}