using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using BenchmarkDotNet.Running;
using SokoSolve.Console.Benchmarks;
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
            System.Console.WriteLine($"{SokoSolveApp.Name} :: v{SokoSolveApp.Version}");
            System.Console.WriteLine(DevHelper.FullDevelopmentContext());
            System.Console.WriteLine("----------------------------------------------------");
            System.Console.WriteLine();

            var root = new RootCommand();
            root.AddCommand(BenchmarkCommand.GetCommand());
            
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
            
            root.Add(AnalyseCommand.GetCommand());
            
            return root.Invoke(args);
        }
    }

    public static class MicroCommand
    {
        public static void Run(string target)
        {
            // var modules = typeof(MicroCommand).Assembly.GetModules(false);
            // var targetType = modules.SelectMany(x => x.FindTypes(Module.FilterTypeName, target)); 
            // var summary = BenchmarkRunner.Run(targetType.Select(x=> BenchmarkRunInfo.);)

            switch (target)
            {
                case "BaseLineSolvers" : 
                    BenchmarkRunner.Run<BaseLineSolvers>();
                    break;
                
               
                
                default: throw new ArgumentException("target");
            }
            
        }
    }
}
