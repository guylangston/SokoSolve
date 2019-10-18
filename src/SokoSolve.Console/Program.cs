using System.IO;
using System.Linq;
using ConsoleZ.Win32;
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

            if (verb == "Play" || verb == "default")
            {
                RunPlay();
            }

            
        }

        private static void RunPlay()
        {
            var consGame = new SimpleConsoleGameClient();
            consGame.Init();
            while (!consGame.Step())
            {
                
            }
            
            
        }

        private static void RunSolve()
        {
            var pathHelper = new PathHelper();
            var lib = new LibraryComponent(pathHelper.GetDataPath());

            var solverRun = new SolverRun();
            solverRun.Load(lib, "SolverRun-Default.tff");

            var exitRequested = false;
            var solverCommand = new SolverCommand
            {
                ExitConditions = ExitConditions.OneMinute,
                CheckAbort = x => exitRequested
            };

            System.Console.WriteLine("See ./solver.txt for a more detailed report.");
            using var report = File.CreateText("solver.txt");
            System.Console.CancelKeyPress += (o, e) =>
            {
                report.Flush();
                System.Console.WriteLine("Ctrl+C detected; cancel requested");

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