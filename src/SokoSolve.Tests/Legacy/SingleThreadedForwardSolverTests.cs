using System;
using System.IO;
using SokoSolve.Core;
using SokoSolve.Core.Solver;
using Xunit;

namespace SokoSolve.Tests.Legacy
{
    public class SingleThreadedForwardSolverTests
    {
        private void PuzzleShouldHaveSolution(ISolver solver, Puzzle puzzle, ExitConditions exit = null,
            bool verbose = false)
        {
            if (exit == null)
                exit = new ExitConditions
                {
                    Duration = TimeSpan.FromSeconds(60),
                    StopOnSolution = true,
                    TotalNodes = int.MaxValue,
                    TotalDead = int.MaxValue
                };
            var command = new SolverCommand
            {
                Puzzle = puzzle,
                Report = TextWriter.Null,
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
            Assert.NotNull(result.Solutions);
            Assert.True(result.HasSolution);
            

            foreach (var sol in result.Solutions)
            {

                string error = null;
                Assert.True(SolverHelper.CheckSolution(command.Puzzle, sol, out error),
                    "Solution is INVALID! " + error);
            }
        }
        
        [Xunit.Fact]
        public void T001_Trivial()
        {
            PuzzleShouldHaveSolution(
                new SingleThreadedForwardSolver(),
                Puzzle.Builder.FromLines(new[]
                {
                    "##########",
                    "#O...X...#",
                    "#O..XPX.O#",
                    "##########"
                }));
        }

        [Xunit.Fact]
        public void T002_BaseLine()
        {
            PuzzleShouldHaveSolution(
                new SingleThreadedForwardSolver(),
                Puzzle.Builder.DefaultTestPuzzle());
        }
    }
}