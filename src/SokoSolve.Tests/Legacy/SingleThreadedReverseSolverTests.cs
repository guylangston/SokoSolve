using System;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Solver;
using Xunit;
using Console = System.Console;
using ExitConditions = SokoSolve.Core.Solver.ExitConditions;

namespace SokoSolve.Tests.Legacy
{
    public class SingleThreadedReverseSolverTests
    {
        private SolverState PerformStandardTest(Puzzle puzzle, ExitConditions exit = null)
        {
            exit = exit ?? new ExitConditions
            {
                Duration = TimeSpan.FromSeconds(60),
                StopOnSolution = true,
                TotalNodes = int.MaxValue,
                TotalDead = int.MaxValue
            };
            // arrange
            var solver = new SingleThreadedReverseSolver(new SolverNodeFactoryTrivial());
            var command = new SolverCommand
            {
                Puzzle = puzzle.Clone(),
                Report = Console.Out,
                ExitConditions = exit
            };

            // act 
            var result = solver.Init(command);
            solver.Solve(result);
            Console.WriteLine(result.ExitDescription);
            Console.WriteLine(SolverHelper.GenerateSummary(result));
            result.ThrowErrors();

            // assert    
            Assert.NotNull(result);

            foreach (var solution in result.SolutionsNodes)
            {
                var p = solution.PathToRoot().ToList();
                p.Reverse();
            }

            foreach (var sol in result.Solutions)
            {
                Console.WriteLine("Path: {0}", sol);
                string error = null;

                Assert.True(SolverHelper.CheckSolution(command.Puzzle, sol, out error),
                    "Solution is INVALID! " + error);
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
    }
}