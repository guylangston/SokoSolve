using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            System.Console.WriteLine("====================================================");
            System.Console.WriteLine($"{Application.Name} :: v{Application.Version}");
            System.Console.WriteLine(DevHelper.FullDevelopmentContext());
            System.Console.WriteLine("----------------------------------------------------");
            System.Console.WriteLine();

            var root = new RootCommand();
            var bench = new Command("benchmark", "Benchmark a single puzzle")
            {
                new Option<int>(new[] {"--time", "-t"}, "TimeOut after (minutes)")
                {
                    Name = "Time",
                },
                new Argument<string>( () => "SQ~P5")
                {
                    Name = "Puzzle",
                    Description = "Puzzle Identifier in the form LIB~PUZ"
                }
            }; 
            bench.Handler = CommandHandler.Create<int, string>(BenchmarkCommand.Run);
            root.AddCommand(bench);
            
            //------------------------------------------------------------------------------------------------------------
            var play = new Command("play", "Play SokoSolve game in the console")
            {
                Handler = CommandHandler.Create(PlayCommand.Run)
            };
            root.AddCommand(play);
            
            //------------------------------------------------------------------------------------------------------------
            var batch = new Command("batch", "Solve a batch of puzzles in one run")
            {
                new Option<string>(new []{"-l", "--lib"})
                {
                    Description = "Library Identifier"
                }
            };
            batch.Handler = CommandHandler.Create<string>(BatchCommand.Run);
            root.AddCommand(batch);
            
            return root.Invoke(args);
        }
    }
}
