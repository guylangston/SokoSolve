using System;
using BenchmarkDotNet.Running;
using SokoSolve.Console.Benchmarks;

namespace SokoSolve.Console
{
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
                
                
                case "pool" : 
                    BenchmarkRunner.Run<NodeLookupMicro>();
                    break;
                
                case "fill" : 
                    BenchmarkRunner.Run<FloodFillMicro>();
                    break;
               
                
                default: throw new ArgumentException("target");
            }
            
        }
    }
}