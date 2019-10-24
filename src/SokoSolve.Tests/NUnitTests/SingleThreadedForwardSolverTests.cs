using System;
using NUnit.Framework;
using SokoSolve.Core.Game;
using SokoSolve.Core.Solver;

namespace SokoSolve.Tests.NUnitTests
{
    [TestFixture]
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
                Report = Console.Out,
                Progress = new ConsoleProgressNotifier(),
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

            foreach (var sol in result.GetSolutions())
            {
                Console.WriteLine("Path: {0}", sol);
                string error = null;

                Assert.That(SolverHelper.CheckSolution(command.Puzzle, sol, out error),
                    "Solution is INVALID! " + error);
            }
        }
        
        [Test]
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

        [Test]
        public void T002_BaseLine()
        {
            PuzzleShouldHaveSolution(
                new SingleThreadedForwardSolver(),
                Puzzle.Builder.DefaultTestPuzzle());
        }
    }
}