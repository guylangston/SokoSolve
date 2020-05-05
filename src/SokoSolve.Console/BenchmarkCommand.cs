using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Reporting;
using SokoSolve.Core.Solver;
using SokoSolve.Game;
using TextRenderZ;
using TextRenderZ.Reporting;

namespace SokoSolve.Console
{
    internal  class BenchmarkCommand
    {
        
        public static Command GetCommand()
        {
            var bench = new Command("benchmark", "Benchmark a single puzzle")
            {
                new Argument<string>( () => "SQ1~P5")
                {
                    Name        = "puzzle",
                    Description = "Puzzle Identifier in the form LIB~PUZ (can be regex)"
                },
                new Option<int>(new[] {"--min", "-m"}, "TimeOut in minutes")
                {
                    Name = "min",
                },
                new Option<int>(new[] {"--sec", "-s"}, "TimeOut in seconds")
                {
                    Name = "sec",
                },
                new Option<string>(new[] {"--solver", "-t"}, "Solver Strategy")
                {
                    Name = "solver",
                },
                new Option<string>(new[] {"--pool", "-p"}, "ISolverPool Type")
                {
                    Name = "pool",
                }, 
                new Option<double>(new[] {"--max-rating", "-maxR"},  "Max Puzzle Rating")
                {
                    Name = "maxR",
                },
                new Option<double>(new[] {"--min-rating", "-minR"},  "Min Puzzle Rating")
                {
                    Name = "minR",
                },
                new Option<string>(new[] {"--save"},   "Save tree to file")
                {
                    Name = "save",
                }
            };
            bench.Handler = HandlerDescriptor.FromMethodInfo(
                                                 typeof(BenchmarkCommand).GetMethod("Run"),
                                                 new BenchmarkCommand())
                                             .GetCommandHandler(); 
            return bench;
        }
        
        public void Run(
            string puzzle = "SQ1~P5", int min = 0, int sec = 0, 
            string solver = "fr!", string pool = BatchSolveComponent.PoolDefault, 
            double minR = 0, double maxR = 2000, string save = null)
        {
            
            if (min == 0 && sec == 0) min = 3;
            
            var pathHelper = new PathHelper();
            var compLib = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));

            var selection = compLib.GetPuzzlesWithCachingUsingRegex(puzzle).ToArray();
            if (!selection.Any())
            {
                throw new Exception($"Not puzzles found '{puzzle}', should be SQ1~P5 or SQ1, etc"); 
            }
            
            var solverRun = new SolverRun();
            solverRun.Init();
            solverRun.AddRange(
                selection
                  .OrderBy(x=>x.Rating)
                  .Where(x=>x.Rating >= minR && x.Rating <= maxR)
            );

            var batch = new BatchSolveComponent();
            batch.SolverRun(new BatchSolveComponent.BatchArgs(puzzle, min, sec, solver, pool, minR, maxR, save)
            {
                Console = System.Console.Out
            }, solverRun);
        }

     
    
    }
}