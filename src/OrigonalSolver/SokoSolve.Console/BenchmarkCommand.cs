using System;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using SokoSolve.Core.Solver.Components;
using TextRenderZ.Reporting;

namespace SokoSolve.Console
{
    internal  class BenchmarkCommand
    {
        public int RunnerAlt(string[] args)
        {
            try
            {
                var parsedArgs = SimpleArgs.FromMetaAndCommandLine(SolverBuilder.Arguments, args, "puzzle", out var defaults);

                // Show only the changes to defaults
                System.Console.WriteLine("|ARGS| " + parsedArgs.GenerateCommandLine(defaults));

                var puzzle = parsedArgs["puzzle"];
                var minR   = double.Parse(parsedArgs["minR"]);
                var maxR   = double.Parse(parsedArgs["maxR"]);
                var cat      = bool.Parse(parsedArgs["cat"]);

                var pathHelper = new PathHelper();
                var compLib    = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));
                var repSol     = new JsonSokobanSolutionRepository(pathHelper.GetRelDataPath("Lib/solutions.json"));

                var selection = compLib.GetPuzzlesWithCachingUsingRegex(puzzle).ToArray();
                if (!selection.Any())
                {
                    throw new Exception($"No puzzles found '{puzzle}', should be {SolverBuilder.LargestRegularlySolvedPuzzle} or SQ1, etc");
                }

                var solverRun = new SolverRun();
                solverRun.Init();
                solverRun.AddRange(
                    selection
                        .OrderBy(x=>x.Rating)
                        .Where(x=>x.Rating >= minR && x.Rating <= maxR)
                );

                var progress = new TextWriterAdapter(System.Console.Out);
                var batch = new BatchSolveComponent(progress, compLib, repSol)
                {
                    CatReport = cat
                };
                batch.SolverRun(solverRun, parsedArgs);

                return 0;
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e);
                return -1;
            }
        }
    }
}
