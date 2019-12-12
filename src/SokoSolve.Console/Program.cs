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

            if (verb == "Solve")
            {
                RunSolve();
            }
            else if (verb == "Play" || verb == "default")
            {
                RunPlay();
            }
            else if (verb == "Bench")
            {
                var summary = BenchmarkRunner.Run<BaseLineSolvers>();
            }
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
            string libName = "Lib\\SokoSolve-v1\\Microban.ssx";

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