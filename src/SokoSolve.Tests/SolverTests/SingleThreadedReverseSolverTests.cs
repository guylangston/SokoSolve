using System;
using System.IO;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Debugger;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using SokoSolve.Core.Solver.NodeFactory;
using SokoSolve.Core.Solver.Solvers;
using Xunit;
using Xunit.Abstractions;
using Console = System.Console;
using ExitConditions = SokoSolve.Core.Solver.ExitConditions;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Tests.SolverTests
{
    public class XUnitOutput : ITextWriter
    {
        private readonly ITestOutputHelper outp;

        public XUnitOutput(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        public void WriteLine(string s)
        {
            outp.WriteLine(s);
        }

        public void Write(string s)
        {
            outp.WriteLine(s);
        }
    }
    
    public class SingleThreadedReverseSolverTests
    {
        private ITestOutputHelper outp;

        public SingleThreadedReverseSolverTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        private SolverState PerformStandardTest(
            Puzzle puzzle, 
            ExitConditions? exit = null, 
            Action<SolverState>? checkAfterInit = null,
            Func<SolverNode, bool>? inspector = null,
            IDebugEventPublisher? debugger = null)
        {
            exit ??= new ExitConditions
            {
                Duration       = TimeSpan.FromSeconds(60),
                StopOnSolution = true,
                MaxNodes     = int.MaxValue,
                MaxDead      = int.MaxValue
            };
            // arrange
            var command = new SolverCommand(puzzle, PuzzleIdent.Temp(),  exit, SolverContainerByType.DefaultEmpty)
            {
                Report = new XUnitOutput(outp),
                Inspector = node =>
                {
                    if (inspector != null && inspector(node))
                    {
                        return true;
                    }
                    return false;
                },
                Debug = debugger
            };
            var solver = new SingleThreadedReverseSolver(new SolverNodePoolingFactoryDefault());
            

            // act 
            var result = solver.Init(command);
            if (checkAfterInit != null)
            {
                checkAfterInit(result);
            }
            solver.Solve(result);
            Assert.NotNull(result);
            
            result.ThrowErrors();
            if (result.HasSolution)
            {
                foreach (var sol in result.Solutions)
                {
                    Assert.True(SolverHelper.CheckSolution(command.Puzzle, sol, out var error), 
                        "Solution is INVALID! => " + error);
                }
            }

            return result;
        }

        [Fact]
        public void R001_Regression_CheckPath()
        {
            var puzzle = Puzzle.Builder.FromLines(new[]
            {
                "##########",
                "#O...X...#",
                "#O..XPX.O#",
                "##########"
            });

            string desc = null;

            SolverHelper.CheckSolution(puzzle, new Path("RRLLLLLRRRRULLLL"), out desc);
            Assert.Null(desc);
        }


        [Fact]
        public void R001_Regression_NotFindingValidPulls()
        {
            var pTxt =
@"###########~
#P..#..#..#~
#..X#X...X#~
##..#OO#..#~
~#..#OO#..#~
~#..#OO#..##
~#X...X#X..#
~#..#..#...#
~###########";

            var res = PerformStandardTest(Puzzle.Builder.FromMultLine(pTxt), new ExitConditions
            {
                Duration = TimeSpan.FromSeconds(10),
                StopOnSolution = true,
                MaxNodes = 2000,
                MaxDead = int.MaxValue
            });
            Assert.True(res.GlobalStats.TotalNodes > 1000);
        }

        [Fact]
        public void R003_DireLagoon_R65_FinalPushNotBeingMade()
        {
            var res = PerformStandardTest(Puzzle.Builder.FromMultLine(
                @"~~~####~~~~
####..#~~~~
#.....####~
#.X.#..O.##
#..#...O..#
##.#XX#O..#
##....#####
#.P.###~~~~
#...#~~~~~~
#####~~~~~~"));
            Assert.True(res.HasSolution);
        }

        [Fact]
        public void R004_SlimyGrave_R93_InvalidStartingPosition()
        {
            var res = PerformStandardTest(Puzzle.Builder.FromMultLine(
                @"~##~#####
##.##.O.#
#.##.XO.#
~##.X...#
##.XP.###
#.X..##~~
#OO.##.##
#...#~##~
#####~#~~"));
            Assert.True(res.HasSolution);
        }

        [Fact]
        public void T001_Trivial()
        {
            var res = PerformStandardTest(Puzzle.Builder.FromLines(new[]
            {
                "##########",
                "#O...X...#",
                "#O..XPX.O#",
                "##########"
            }));

            Assert.True(res.HasSolution);
        }
        
        

        [Fact]
        public void T002_DefaultTest_HasSolutions()
        {
            var res = PerformStandardTest(Puzzle.Builder.DefaultTestPuzzle(), 
                checkAfterInit: (initState) => {
                    var ss = (SolverStateSingleTree)initState;
                    foreach (var kid in ss.TreeState.Root.Children)
                    {
                        this.outp.WriteLine($"pb:{kid.PlayerBefore} pa:{kid.PlayerAfter} cb:{kid.CrateBefore} ca:{kid.CrateAfter}");
                    }
                    Assert.Equal(2, ss.TreeState.Root.Children.Count());
                },
                debugger: new FuncDebugEventPublisher(
                    (e) => {
                        outp.WriteLine(e.ToString());
                    },
                    (ee) => {

                        outp.WriteLine(ee.ctx[1].ToString());
                        var node = (SolverNode)ee.ctx[0];
                        var path = node.PathToRoot().Reverse();
                        foreach (var nn in path.Take(3))
                        {
                            outp.WriteLine(nn.CrateMap.ToString());    
                            outp.WriteLine(nn.MoveMap.ToString());
                            outp.WriteLine("--------------------------");
                            
                        }
                        
                    }
                    )
            );
            
            Assert.True(res.HasSolution);
        }
        
        [Fact]
        public void R005_SQ1_P41_Edgy_Grave_BadTree()
        {
            // BUG: Only generated 1 node, then stops eval 
            var puzzle = Puzzle.Builder.FromLines(new[] {
                "#####~########~",
                "#...###.O.X..#~",
                "#...X.$OO.#X.##",
                "##.X#.OO$.X..P#",
                "~#..X.O.###...#",
                "~########~#####",
            });
            var res = PerformStandardTest(puzzle, new ExitConditions()
            {
                MaxNodes = 100
            });
            Assert.True(res.GlobalStats.TotalNodes >= 100);
        }
        
        [Fact]
        public void R006_MBP41_InvalidSolutions()
        {
            var puzzle = Puzzle.Builder.FromLines(new[] {
                "####~~~",
                "#..####",
                "#.O.O.#",
                "#.XX#P#",
                "##....#",
                "~######",
            });
            var res = PerformStandardTest(puzzle);
            Assert.True(res.HasSolution);
        }
    }
}