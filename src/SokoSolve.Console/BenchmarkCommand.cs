using System.CommandLine;
using System.CommandLine.Invocation;
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
                new Argument<string>( () => "SQ~P5")
                {
                    Name        = "puzzle",
                    Description = "Puzzle Identifier in the form LIB~PUZ"
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
            bench.Handler = CommandHandler.Create<int, int, string, string>(Run);
            return bench;
        }
        
        public static void Run(int min = 3, int sec = 0, string puzzle = "SQ1~P5", string solver = "fr!")
        {
            var pathHelper = new PathHelper();
            var compLib = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));
            
            var solverRun = new SolverRun();
            var pz = compLib.GetPuzzleWithCaching(PuzzleIdent.Parse(puzzle));
            solverRun.Init();
            solverRun.Add(pz);
            
            CommonSolverCommand.SolverRun(min, sec, solver, solverRun);
        }

      
    }
}