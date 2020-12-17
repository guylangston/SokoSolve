using System;
using System.IO;
using System.Linq;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests.SolverTests
{
    public class SingleThreadedForwardSolverTests
    {
        private ITestOutputHelper outp;
        
        public SingleThreadedForwardSolverTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        private void PuzzleShouldHaveSolution(ISolver solver, Puzzle puzzle, 
            ExitConditions? exit = null,
            bool verbose = false)
        {
            var command = CreateCommand(puzzle, exit);

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

        private SolverCommand CreateCommand(Puzzle p, ExitConditions? exit = null) 
            => new SolverCommand(
                p, 
                PuzzleIdent.Temp(), 
                exit ?? new ExitConditions
                {
                    Duration       = TimeSpan.FromSeconds(60),
                    StopOnSolution = true,
                    TotalNodes     = int.MaxValue,
                    TotalDead      = int.MaxValue
                }, SolverContainerByType.DefaultEmpty)
                {
                    Report = new XUnitOutput(outp)
                };

       
        
        [Xunit.Fact]
        public void NoSolutions()
        {

            var command = CreateCommand(Puzzle.Builder.FromLines(new[]
                {
                    "##########",
                    "#O....X..#",
                    "#O..P..X.#",
                    "#O....X..#",
                    "##########"
                }),
                ExitConditions.OneMinute());
            
            var solver = new SingleThreadedForwardSolver(command, new SolverNodePoolingFactoryDefault());
            var state  = solver.Init(command) as SolverBaseState;
            var result = solver.Solve(state);
            
            Assert.True(state.Solutions == null || state.Solutions.Count == 0);
            Assert.NotEmpty(state.Root.Children);

            foreach (var n in state.Root.Recurse())
            {
                outp.WriteLine(n.ToString());
            }
            Assert.Equal(4, state.Root.CountRecursive()); // NOTE: Should this not be 5 = 2 valid pushes, then 3 dead
            Assert.True( state.Root.Recurse().All(x=>((SolverNode)x).IsClosed));
            Assert.Equal(ExitResult.ExhaustedTree, result);
        }
        
        [Xunit.Fact]
        public void Exhause_Default()
        {
            var command = CreateCommand(
                Puzzle.Builder.DefaultTestPuzzle(),
                new ExitConditions()
                {
                    StopOnSolution = false,
                    Duration       = TimeSpan.FromMinutes(5)
                });
            var solver = new SingleThreadedForwardSolver(command, new SolverNodePoolingFactoryDefault());
            var state  = solver.Init(command) as SolverBaseState;
            var result = solver.Solve(state);
            
            Assert.Equal(ExitResult.ExhaustedTree, result);
            
            var firstOpen = state.Root.Recurse().FirstOrDefault(x=>x.IsOpen);
            Assert.Null( firstOpen);
            
            
            Assert.True(state.Queue is SolverQueue);
            if (state.Queue is SolverQueue qq)
            {
                Assert.Equal(0, qq.Count);    
            }
        }
        
        [Xunit.Fact]
        public void NoSolutions_InvalidPuzzle()
        {
            var command = CreateCommand(
                Puzzle.Builder.FromLines(new[]
                {
                    // More goals than crates - strictly not valid
                    "##########",
                    "#O...X..O#",    
                    "#O..XPX.O#",
                    "#O..X.X.O#",
                    "##########"
                }),
                new ExitConditions()
                {
                    StopOnSolution = false,
                    Duration       = TimeSpan.FromMinutes(1)
                });
            

            var solver = new SingleThreadedForwardSolver(command, new SolverNodePoolingFactoryDefault());
            Assert.Throws<InvalidDataException>(() =>
            {
                var state = solver.Init(command) as SolverBaseState;
            });
            
            
        }
        
        
       
    }
}