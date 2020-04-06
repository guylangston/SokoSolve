using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console
{
    
    
    internal static class BatchCommand
    {

        public static Command GetCommand()
        {
            var batch = new Command("batch", "Solve a batch of puzzles in one run")
            {
                new Option<string>(new []{"-l", "--lib"})
                {
                    Name        = "lib",
                    Description = "Library Identifier"
                },
                new Option<int>(new[] {"--min", "-m"}, "TimeOut after (minutes)")
                {
                    Name = "min",
                },
                new Option<int>(new[] {"--sec", "-s"}, "TimeOut after (minutes)")
                {
                    Name = "sec",
                },
                new Option<string>(new[] {"--solver", "-t"}, "Solver Strategy: "+CommonSolverCommand.Help)
                {
                    Name = "solver",
                }
            };
            batch.Handler = CommandHandler.Create<string, int, int, string>(BatchCommand.Run);
            return batch;
        }
        
        public static void Run(string lib = "SQ1", int min = 1, int sec = 0, string solver = "fr!")
        {
            var pathHelper = new PathHelper();
            var compLib    = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));

            var solverRun = new SolverRun();
            solverRun.Load(compLib.GetLibraryWithCaching(lib)
                                  .Where(x => x.Rating > 100 && x.Rating < 1500));
            
            CommonSolverCommand.SolverRun(min, sec, solver, solverRun);
        }
    }
}