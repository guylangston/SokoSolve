using System;
using System.IO;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Solver;
using Xunit;
using Console = System.Console;

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
                new SingleThreadedForwardSolver(new SolverNodeFactoryTrivial()),
                Puzzle.Builder.FromLines(new[]
                {
                    "##########",
                    "#O...X...#",
                    "#O..XPX.O#",
                    "##########"
                }));
        }
        
        [Xunit.Fact]
        public void NoSolutions()
        {
            
            var command = new SolverCommand
            {
                Puzzle         = Puzzle.Builder.FromLines(new[]
                {
                    "##########",
                    "#O.....X.#",    
                    "#O..P..X.#",
                    "#O.....X.#",
                    "##########"
                }),
                Report         = TextWriter.Null,
                ExitConditions = ExitConditions.OneMinute()
            };

            var solver = new SingleThreadedForwardSolver(new SolverNodeFactoryTrivial());
            var state = solver.Init(command) as SolverBaseState;
            var result = solver.Solve(state);
            
            Assert.Empty(state.Solutions);
            Assert.True( state.Root.All(x=>((SolverNode)x).IsClosed));
            Assert.Equal(ExitConditions.Conditions.ExhaustedTree, result);
            
            
        }
        
        [Xunit.Fact]
        public void NoSolutions_InvalidPuzzle()
        {
            
            var command = new SolverCommand
            {
                Puzzle = Puzzle.Builder.FromLines(new[]
                {
                    // More goals than crates - strictly not valid
                    "##########",
                    "#O...X..O#",    
                    "#O..XPX.O#",
                    "#O..XPX.O#",
                    "##########"
                }),
                Report         = TextWriter.Null,
                ExitConditions = ExitConditions.OneMinute()
            };

            var solver = new SingleThreadedForwardSolver(new SolverNodeFactoryTrivial());
            var state  = solver.Init(command) as SolverBaseState;
            var result = solver.Solve(state);
            
            Assert.Empty(state.Solutions);
            Assert.True( state.Root.All(x=>((SolverNode)x).IsClosed));
            Assert.Equal(ExitConditions.Conditions.ExhaustedTree, result);
            
            
        }
        
        
        [Xunit.Fact]
        public void T002_BaseLine()
        {
            PuzzleShouldHaveSolution(
                new SingleThreadedForwardSolver(new SolverNodeFactoryTrivial()),
                Puzzle.Builder.DefaultTestPuzzle());
        }
    }
}