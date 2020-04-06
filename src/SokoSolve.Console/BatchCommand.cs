using System.IO;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console
{
    internal static class BatchCommand
    {
        public static void Run(string lib = "SQ1")
        {
            var pathHelper = new PathHelper();
            var compLib = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));

            var solverRun = new SolverRun();
            solverRun.Load(compLib.GetLibraryWithCaching(lib)
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