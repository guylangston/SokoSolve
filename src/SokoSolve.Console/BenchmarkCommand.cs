using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using TextRenderZ;

namespace SokoSolve.Console
{
    internal  class BenchmarkCommand
    {
        
        public static Command GetCommand()
        {
            var bench = new Command("benchmark", "Benchmark a single puzzle")
            {
                new Argument<string>( () => SolverBuilder.LargestRegularlySolvedPuzzleId)
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
                new Option<string>(new[] {"--queue", "-q"}, "ISolverQueue Type")
                {
                    Name = "queue",
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
                },
                new Option<bool>(new[] {"--cat"},    "Print Report to stdout")
                {
                    Name = "cat",
                }
            };
            bench.Handler = HandlerDescriptor.FromMethodInfo(
                typeof(BenchmarkCommand).GetMethod(nameof(Run)),
                new BenchmarkCommand()
                ).GetCommandHandler(); 
            return bench;
        }

        
        
        public void Run(
            string puzzle = SolverBuilder.LargestRegularlySolvedPuzzleId, 
            int min = 0, 
            int sec = 0, 
            string solver = "fr!", 
            string pool = SolverBuilder.LookupFactoryDefault,
            string queue = SolverBuilder.QueueFactoryDefault,
            double minR = 0, 
            double maxR = 2000, 
            string? save = null,
            bool cat = false
            )
        {
            if (min == 0 && sec == 0) min = 3;
            
            var pathHelper = new PathHelper();
            var compLib = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));

            var selection = compLib.GetPuzzlesWithCachingUsingRegex(puzzle).ToArray();
            if (!selection.Any())
            {
                throw new Exception($"Not puzzles found '{puzzle}', should be {SolverBuilder.LargestRegularlySolvedPuzzleId} or SQ1, etc"); 
            }
            
            var solverRun = new SolverRun();
            solverRun.Init();
            solverRun.AddRange(
                selection
                  .OrderBy(x=>x.Rating)
                  .Where(x=>x.Rating >= minR && x.Rating <= maxR)
            );

            var batch = new BatchSolveComponent(compLib, new TextWriterAdapter(System.Console.Out), null)
            {
                CatReport = cat
            };
            batch.SolverRun(solverRun, new Dictionary<string, string>()
            {
                {"solver",solver},
                {"pool",pool},
                {"queue",queue},
                {"min",min.ToString()},
                {"sec",sec.ToString()},
            });
        }

        public int RunnerAlt(string[] args)
        {
            try
            {
                
                var aa = new Dictionary<string, string>(SolverBuilder.Defaults);
                SetFromCommandLind(aa, args);
                
                // Show only the changes to defaults
                System.Console.WriteLine("ARGS: " + FluentString.Join(aa.Where(x=> 
                    {
                        if (SolverBuilder.Defaults.TryGetValue(x.Key, out var y))
                        {
                            return y != x.Value;
                        }
                        return true;
                    }),
                    new JoinOptions()
                    {
                        Sep = " ",
                        WrapAfter = 100
                    }, (s, pair) => s.Append($"--{pair.Key} {pair.Value}")));

                var puzzle = aa["puzzle"];
                var minR   = double.Parse(aa["minR"]);
                var maxR   = double.Parse(aa["maxR"]);
                var cat      = bool.Parse(aa["cat"]);
                
                var pathHelper = new PathHelper();
                var compLib    = new LibraryComponent(pathHelper.GetRelDataPath("Lib"));

                var selection = compLib.GetPuzzlesWithCachingUsingRegex(puzzle).ToArray();
                if (!selection.Any())
                {
                    throw new Exception($"Not puzzles found '{puzzle}', should be {SolverBuilder.LargestRegularlySolvedPuzzleId} or SQ1, etc"); 
                }
            
                var solverRun = new SolverRun();
                solverRun.Init();
                solverRun.AddRange(
                    selection
                        .OrderBy(x=>x.Rating)
                        .Where(x=>x.Rating >= minR && x.Rating <= maxR)
                );

                var batch = new BatchSolveComponent(compLib, new TextWriterAdapter(System.Console.Out), null)
                {
                    CatReport = cat
                };
                batch.SolverRun(solverRun, aa);

                return 0;
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e);
                return -1;
            }
        }
        
        private void SetFromCommandLind(Dictionary<string, string> aa, string[] args)
        {
            var cc = 0;
            while (cc < args.Length)
            {
                if (args[cc].StartsWith("--"))
                {
                    var name = args[cc].Remove(0, 2);
                    if (cc + 1 < args.Length && !args[cc+1].StartsWith("--"))
                    {
                        aa[name] = args[cc + 1];
                        cc++;
                    }
                    else
                    {
                        aa[name] = true.ToString();  // flag
                    }
                }
                else if (cc == 0)
                {
                    aa["puzzle"] = args[0]; // default 1st puzzle
                }
                cc++;
            }
        }



    }
}