using System;
using SokoSolve.Core.Game;
using SokoSolve.Core.Solver;
using Xunit;

namespace SokoSolve.Tests.NUnitTests
{
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
            Assert.NotNull(result);

            Assert.True(result.HasSolution);
            Assert.NotNull(result.GetSolutions());
            Assert.NotEmpty(result.GetSolutions());

            foreach (var sol in result.GetSolutions())
            {
                Console.WriteLine("Path: {0}", sol);
                string error = null;
                Assert.True(SolverHelper.CheckSolution(command.Puzzle, sol, out error),
                    "Solution is INVALID! " + error);
            }

            return result;
        }


        [Xunit.Fact]
        public void T001_Trivial()
        {
            var res = PerformStandardTest(Puzzle.Builder.FromMultLine(
@"##########
#O...X...#
#O..XPX.O#
##########"
            ));

            Assert.True(res.HasSolution);
        }

        [Xunit.Fact]
        public void T002_BaseLine()
        {
            var res = PerformStandardTest(Puzzle.Builder.DefaultTestPuzzle());

            Assert.True(res.HasSolution);
        }

        [Xunit.Fact]
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

            Assert.True(res.HasSolution);
        }
    }
}