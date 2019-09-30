using System;
using SokoSolve.Core;
using SokoSolve.Core.Library;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine($"{Host.Name} - v{Host.Version}");
            System.Console.WriteLine();
            

            var pathHelper = new PathHelper();
            var lib = new LibraryComponent(pathHelper.GetDataPath());
            var runner = new SolverRunComponent();
            
            var solverRun = new SolverRun();
            solverRun.Load(lib, "SolverRun-Default.tff");


            bool exitRequested = false;
            System.Console.CancelKeyPress += (o, e) =>
            {
                System.Console.WriteLine("Ctrl+C detected; cancel requested");
                exitRequested = true;
            };


            var solverCommand = new SolverCommand()
            {
                ExitConditions = ExitConditions.OneMinute,
                CheckAbort = x=>exitRequested
            };
            runner.Run(solverRun, solverCommand, new MultiThreadedForwardReverseSolver());
        }
    }
}
