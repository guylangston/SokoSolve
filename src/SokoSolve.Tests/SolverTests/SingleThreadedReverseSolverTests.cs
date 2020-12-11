using System;
using System.IO;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Solver;
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

        private SolverState PerformStandardTest(Puzzle puzzle, ExitConditions exit = null)
        {
            exit ??= new ExitConditions
            {
                Duration       = TimeSpan.FromSeconds(60),
                StopOnSolution = true,
                TotalNodes     = int.MaxValue,
                TotalDead      = int.MaxValue
            };
            // arrange
            var solver = new SingleThreadedReverseSolver(new SolverNodePoolingFactoryDefault());
            var command = new SolverCommand(puzzle.Clone(), exit)
            {
                Report = new XUnitOutput(outp),
                Inspector = node =>
                {
                    if (node.GetHashCode() == 929793)
                    {
                        outp.WriteLine(node.ToString());
                        return true;
                    }

                    return false;
                }
            };

            // act 
            var result = solver.Init(command);
            solver.Solve(result);
            // Console.WriteLine(result.ExitDescription);
            // Console.WriteLine(SolverHelper.GenerateSummary(result));
            result.ThrowErrors();

            // assert    
            Assert.NotNull(result);

            if (result.HasSolution)
            {
                foreach (var solution in result.SolutionsNodes)
                {
                    var p = solution.PathToRoot().ToList();
                    p.Reverse();
                }

                foreach (var sol in result.Solutions)
                {
                    Assert.True(SolverHelper.CheckSolution(command.Puzzle, sol, out var error), "Solution is INVALID! " + error);
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
                TotalNodes = 2000,
                TotalDead = int.MaxValue
            });
            Assert.True(res.Statistics.TotalNodes > 1000);
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
        public void T002_BaseLine()
        {
            var res = PerformStandardTest(Puzzle.Builder.DefaultTestPuzzle());
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
                TotalNodes = 100
            });
            Assert.True(res.Statistics.TotalNodes >= 100);
        }
    }
}