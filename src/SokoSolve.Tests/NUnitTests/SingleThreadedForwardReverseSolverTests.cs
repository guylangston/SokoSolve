using System;
using NUnit.Framework;
using SokoSolve.Core.Game;
using SokoSolve.Core.Solver;

namespace SokoSolve.Tests.NUnitTests
{
    [TestFixture]
    public class SingleThreadedForwardReverseSolverTests
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
            var solver = new SingleThreadedForwardReverseSolver();
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

            Assert.That(result.HasSolution, Is.True);
            Assert.That(result.GetSolutions(), Is.Not.Null);
            Assert.That(result.GetSolutions(), Is.Not.Empty);

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
        public void T001_Trivial()
        {
            var res = PerformStandardTest(Puzzle.Builder.FromMultLine(
@"##########
#O...X...#
#O..XPX.O#
##########"
            ));

            Assert.That(res.HasSolution);
        }

        [Test]
        public void T002_BaseLine()
        {
            var res = PerformStandardTest(Puzzle.Builder.DefaultTestPuzzle());

            Assert.That(res.HasSolution);
        }

        [Test]
        public void T003_SolutionInvalid_FinalMoveMissing()
        {
            var p = @"~##~#####
##.##.O.#
#.##.XO.#
~##.X...#
##.XP.###
#.X..##~~
#OO.##.##
#...#~##~
#####~#~~";
            var res = PerformStandardTest(Puzzle.Builder.FromMultLine(p));

            Assert.That(res.HasSolution);
        }
    }
}