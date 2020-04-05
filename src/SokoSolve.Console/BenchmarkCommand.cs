using System;
using System.Collections.Generic;
using System.IO;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console
{
    internal static class BenchmarkCommand
    {
        public static void Run(int time = 3, string ident = "SQ1~P5")
        {
            var exitRequested = false;
            
            var pathHelper = new PathHelper();
            var compLib = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));
            
            var solverRun = new SolverRun();
            var puzzle = compLib.GetPuzzleWithCaching(PuzzleIdent.Parse(ident));
            solverRun.Init();
            solverRun.Add(puzzle);

            var ioc = new SolverContainerByType(new Dictionary<Type, Func<Type, object>>()
            {
                { typeof(ISolverNodeLookup), (t) =>  new SolverNodeLookupDoubleBuffered(new SolverNodeLookupLinked(new SolverNodeLookupLongTerm()))},
                { typeof(ISolverQueue), (t) =>  new SolverQueueConcurrent()},
            });
            
            var solverCommand = new SolverCommand
            {
                ServiceProvider = ioc,
                ExitConditions = new ExitConditions() 
                { 
                    Duration =  TimeSpan.FromMinutes(time), 
                    StopOnSolution = true 
                },                        
                CheckAbort = x => exitRequested
            };

            var outFile = $"./benchmark--{DateTime.Now:s}.txt".Replace(':', '-');
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
                exitRequested = true;
            };

            var runner = new BatchSolveComponent(report, System.Console.Out);
            
            var summ = runner.Run(solverRun, solverCommand, new MultiThreadedForwardReverseSolver());   
            
        }
            
    }
}