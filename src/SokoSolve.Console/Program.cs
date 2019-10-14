using System.IO;
using SokoSolve.Core;
using SokoSolve.Core.Library;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            System.Console.WriteLine($"{Host.Name} - v{Host.Version}");
            System.Console.WriteLine("====================================================");
            System.Console.WriteLine();

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