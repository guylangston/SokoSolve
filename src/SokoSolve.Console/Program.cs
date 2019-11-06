using System.IO;
using System.Linq;
using BenchmarkDotNet.Running;
using SokoSolve.Console.Benchmarks;
using SokoSolve.Console.Scenes;
using SokoSolve.Core;
using SokoSolve.Core.Library;
using SokoSolve.Core.Solver;

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
            using var master = new MasterGameLoop();
            
            master.Init();
            master.Start();
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