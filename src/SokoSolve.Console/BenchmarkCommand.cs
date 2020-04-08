using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console
{
    internal static class BenchmarkCommand
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
                new Option<string>(new[] {"--solver", "-t"}, "Solver Strategy: "+CommonSolverCommand.SolverFactoryHelp)
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
                }
            }; 
            bench.Handler = CommandHandler.Create<string, int, int, 
                string, string,
                double, double>(Run);
            return bench;
        }
        
        public static void Run(
            string puzzle = "SQ1~P5", int min = 0, int sec = 0, 
            string solver = "fr!", string pool = "bb:ll:lt", 
            double minR = 0, double maxR = 2000)
        {
            if (min == 0 && sec == 0) min = 3;
            
            var pathHelper = new PathHelper();
            var compLib = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));
            
            var solverRun = new SolverRun();
            solverRun.Init();
            solverRun.AddRange(
                compLib.GetPuzzlesWithCachingUsingRegex(puzzle)
                  .OrderBy(x=>x.Rating)
                  .Where(x=>x.Rating >= minR && x.Rating <= maxR)
            );
            
            CommonSolverCommand.SolverRun(min, sec, solver,pool, minR, maxR, solverRun);
        }

      
    }
}