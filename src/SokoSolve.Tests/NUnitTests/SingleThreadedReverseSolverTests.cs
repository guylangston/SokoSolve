using System;
using NUnit.Framework;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Game;
using SokoSolve.Core.Solver;
using ExitConditions = SokoSolve.Core.Solver.ExitConditions;

namespace SokoSolve.Tests.NUnitTests
{
    [TestFixture]
    public class SingleThreadedReverseSolverTests
    {
        private SolverCommandResult PerformStandardTest(Puzzle puzzle, ExitConditions exit = null)
        {
            exit = exit ?? new ExitConditions
            {
                Duration = TimeSpan.FromSeconds(60),
                StopOnSolution = true,
                TotalNodes = int.MaxValue,
                TotalDead = int.MaxValue
            };
            // arrange
            var solver = new SingleThreadedReverseSolver();
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
            Console.WriteLine(SolverHelper.Summary(result));
            result.ThrowErrors();

            // assert    
            Assert.That(result, Is.Not.Null);

            foreach (var solution in result.Solutions)
            {
                var p = solution.PathToRoot();
                p.Reverse();
            }

            foreach (var sol in result.GetSolutions())
            {
                Console.WriteLine("Path: {0}", sol);
                string error = null;

                Assert.That(SolverHelper.CheckSolution(command.Puzzle, sol, out error),
                    "Solution is INVALID! " + error);
            }

            return result;
        }

        [Test]
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
            Assert.That(desc, Is.Null);
        }


        [Test]
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
            Assert.That(res.Statistics.TotalNodes > 1000);
        }

        [Test]
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
            Assert.That(res.HasSolution);
        }

        [Test]
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
            Assert.That(res.HasSolution);
        }

        [Test]
        public void T001_Trivial()
        {
            var res = PerformStandardTest(Puzzle.Builder.FromLines(new[]
            {
                "##########",
                "#O...X...#",
                "#O..XPX.O#",
                "##########"
            }));

            Assert.That(res.HasSolution);
        }

        [Test]
        public void T002_BaseLine()
        {
            var res = PerformStandardTest(Puzzle.Builder.DefaultTestPuzzle());
            Assert.That(res.HasSolution);
        }
    }
}