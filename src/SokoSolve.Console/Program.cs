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
            root.AddCommand(BenchmarkCommand.GetCommand());
            
            //------------------------------------------------------------------------------------------------------------
            var play = new Command("play", "Play SokoSolve game in the console")
            {
                Handler = CommandHandler.Create(PlayCommand.Run)
            };
            root.AddCommand(play);
            //------------------------------------------------------------------------------------------------------------
            
            return root.Invoke(args);
        }
    }
}
