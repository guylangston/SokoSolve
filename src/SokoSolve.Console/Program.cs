using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using SokoSolve.Core;
using SokoSolve.Core.Solver;


namespace SokoSolve.Console
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            System.Console.WriteLine(StringRepeat("-=*#@#*=", System.Console.WindowWidth-1));
            System.Console.WriteLine($"{SokoSolveApp.Name} :: v{SokoSolveApp.Version}");
            System.Console.WriteLine(DevHelper.FullDevelopmentContext());
            System.Console.WriteLine(StringRepeat("-", System.Console.WindowWidth-1));
            System.Console.WriteLine();
            
            if (args.Any() && args.First() == "benchmark")
            {
                var inner = args.Skip(1).ToArray();
                var bench = new BenchmarkCommand();
                return bench.RunnerAlt(inner);      // Allow more control vs strongly typed
            }
            

            var root = new RootCommand();

            //------------------------------------------------------------------------------------------------------------
            var play = new Command("play", "Play SokoSolve game in the console")
            {
                Handler = CommandHandler.Create(PlayCommand.Run)
            };
            root.AddCommand(play);
            //------------------------------------------------------------------------------------------------------------
            var micro = new Command("micro", "Micro Benchmarks (BenchmarkDotNet)")
            {
                new Argument<string>( () => "BaseLineSolvers")
                {
                    Name        = "target",
                    Description = "Target Type"
                },
            };
            micro.Handler = CommandHandler.Create<string>(MicroCommand.Run);
            root.AddCommand(micro);
            //------------------------------------------------------------------------------------------------------------
            var scratch = new Command("scratch", "WIP")
            {
                new Argument<string>( () => "def")
                {
                    Name        = "target",
                    Description = "Target Type"
                },
            };
            scratch.Handler = CommandHandler.Create<string>(ScratchCommand.Run);
            root.AddCommand(scratch);
            //------------------------------------------------------------------------------------------------------------
            
            root.Add(AnalyseCommand.GetCommand());
            
            
            return root.Invoke(args);
        }

        public static string StringRepeat(string s, int count)
        {
            var sb = new StringBuilder();
            while (sb.Length < count)
            {
                sb.Append(s);
            }
            while (sb.Length > count)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }
    }

}
