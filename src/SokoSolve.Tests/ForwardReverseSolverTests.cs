using System;
using NUnit.Framework;
using SokoSolve.Core.Game;
using SokoSolve.Core.Solver;

namespace SokoSolve.Tests
{
    [TestFixture]
    public class ForwardReverseSolverTests
    {
        [Ignore("Scrathpad")]
        [Test]
        public void BasicThreading()
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
            var puzzle = Puzzle.Builder.FromMultLine(pTxt);

            var exit = new ExitConditions
            {
                Duration = TimeSpan.FromSeconds(60),
                StopOnSolution = true,
                TotalNodes = int.MaxValue,
                TotalDead = int.MaxValue
            };
            // arrange
            var solver = new MultiThreadedForwardReverseSolver();
            var command = new SolverCommand
            {
                Puzzle = puzzle.Clone(),
                Report = Console.Out,
                ExitConditions = exit,
                Progress = new ConsoleProgressNotifier()
            };

            // act 
            var result = solver.Init(command);
            solver.Solve(result);
            Assert.That(result, Is.Not.Null);
            Console.WriteLine(result.ExitDescription);
            Console.WriteLine(SolverHelper.Summary(result));
            result.ThrowErrors();

            // assert    
            Assert.That(result, Is.Not.Null);
        }
    }
}