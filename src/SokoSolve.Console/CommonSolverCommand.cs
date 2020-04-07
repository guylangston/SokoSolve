using System;
using System.Collections.Generic;
using System.IO;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console
{
    internal static class CommonSolverCommand
    {
        public static void SolverRun(int min, int sec, string solver, SolverRun solverRun)
        {
            var exitRequested = false;
            var ioc = new SolverContainerByType(new Dictionary<Type, Func<Type, object>>()
            {
                {
                    typeof(ISolverNodeLookup),
                    //t => new SolverNodeLookupSlimRWLock(new SolverNodeLookupSimpleList()) 
                    t => new SolverNodeLookupDoubleBuffered(new SolverNodeLookupSlimRWLock(new SolverNodeLookupSimpleList()))
                    
                },
                {
                    typeof(ISolverQueue),
                    (t) => new SolverQueueConcurrent()
                },
            });

            var solverCommand = new SolverCommand
            {
                ServiceProvider = ioc,
                ExitConditions = new ExitConditions()
                {
                    Duration       = TimeSpan.FromMinutes(min).Add(TimeSpan.FromSeconds(sec)),
                    StopOnSolution = true,
                },
                //Progress = new ConsoleProgressNotifier(),  
                CheckAbort = x => exitRequested
                
            };

            var outFile   = $"./benchmark--{DateTime.Now:s}.txt".Replace(':', '-');
            var outFolder = "./results/";
            if (!Directory.Exists(outFolder)) Directory.CreateDirectory(outFolder);
            var info = new FileInfo(Path.Combine(outFolder, outFile));
            System.Console.WriteLine($"Report: {info.FullName}");
            System.Console.WriteLine();

            using var report = File.CreateText(info.FullName);
            System.Console.CancelKeyPress += (o, e) =>
            {
                report.Flush();
                System.Console.WriteLine("Ctrl+C detected; cancel requested");

                solverCommand.ExitConditions.ExitRequested = true;
                exitRequested                              = true;
            };

            var runner = new BatchSolveComponent(report, System.Console.Out);

            var solverInstance = SolverFactory(solver, ioc);

            var summ = runner.Run(solverRun, solverCommand, solverInstance);
        }

        public const string Help = "f, r, fr, fr!";
        
        private static ISolver SolverFactory(string solver, SolverContainerByType ioc)
        {
            return solver switch
            {
                "f"   => new SingleThreadedForwardSolver(),
                "r"   => new SingleThreadedReverseSolver(),
                "fr"  => new SingleThreadedForwardReverseSolver(),
                "fr!" => new MultiThreadedForwardReverseSolver(),
                _     => throw new Exception($"Unknown Solver '{solver}', try ({Help})")
            };
        }
    }
}