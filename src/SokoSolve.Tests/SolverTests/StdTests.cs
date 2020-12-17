using System;
using System.Collections.Generic;
using SokoSolve.Core;
using SokoSolve.Core.Debugger;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests.SolverTests
{
    public abstract class StdTestsBase
    {
        private readonly ITestOutputHelper outp;

        protected StdTestsBase(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        protected abstract ISolver CreateSolver(SolverCommand cmd);
        protected virtual void InitServiceProvider(SolverContainerByType c)
        {
            // Nothing
        }

        protected virtual SolverState AttemptSolve(Puzzle puzzle,
            ExitConditions? exit = null,
            Action<SolverState>? checkAfterInit = null,
            Func<SolverNode, bool>? inspector = null,
            IDebugEventPublisher? debugger = null)
        {
            exit ??= new ExitConditions
            {
                Duration       = TimeSpan.FromSeconds(10),
                StopOnSolution = true,
                TotalNodes     = int.MaxValue,
                TotalDead      = int.MaxValue
            };
            
            var container = new SolverContainerByType();
            var command = new SolverCommand(puzzle, PuzzleIdent.Temp(), exit, container )
            {
                Report = new XUnitOutput(outp),
                Inspector = inspector,
                Debug = debugger,
            };
            var solver = CreateSolver(command);
            InitServiceProvider(container);

            // act 
            var result = solver.Init(command);
            if (checkAfterInit != null)
            {
                checkAfterInit(result);
            }
            solver.Solve(result);
            result.ThrowErrors();

            // assert    
            Assert.NotNull(result);

            if (result.Solutions != null)
            {
                foreach (var sol in result.Solutions)
                {
                    Assert.True(SolverHelper.CheckSolution(command.Puzzle, sol, out var error), "Solution is INVALID! " + error);
                }    
            }
            return result;
        }

        [Fact]
        public void T001_Trivial_HasSolutions()
        {
            var res = AttemptSolve(Puzzle.Builder.FromMultLine(
                @"##########
#O...X...#
#O..XPX.O#
##########"));

            Assert.True(res.HasSolution);
        }
        
        [Fact]
        public void T002_DefaultPuzzle_HasSolutions()
        {
            var res = AttemptSolve(Puzzle.Builder.DefaultTestPuzzle());
            Assert.True(res.HasSolution);
        }
        
        [Fact]
        public void T003_NoSolutions()
        {
            var puzzle = Puzzle.Builder.FromLines(new[]
            {
                "##########",
                "#O....X..#",
                "#O..P..X.#",
                "#O....X..#",
                "##########"
            });

            var state = AttemptSolve(puzzle); 
            
            Assert.True(state.Solutions == null || state.Solutions.Count == 0);
            // TODO: Assert.NotEmpty(state.Root.Children);
            // TODO: Assert.Equal(4, state.Root.CountRecursive()); // NOTE: Should this not be 5 = 2 valid pushes, then 3 dead
            // TODO: Assert.True( state.Root.Recurse().All(x=>((SolverNode)x).IsClosed));
            Assert.Equal(ExitResult.ExhaustedTree, state.Exit);
        }
        
        [Fact]
        public void T004_Exhause()
        {
            var puzzle = Puzzle.Builder.FromLines(new[]
            {
                "##########",
                "#O....XP.#",
                "#O.....X.#",
                "#O....X..#",
                "##########"
            });
            var state = AttemptSolve(puzzle, new ExitConditions()
            {
                StopOnSolution = false
            });
            
            Assert.NotEmpty(state.Solutions);
            Assert.Equal(ExitResult.ExhaustedTree, state.Exit);
            
            // TODO Confirm all nodes are !Open
        }
    }

    public abstract class StdTestsMultiBase : StdTestsBase
    {
        protected StdTestsMultiBase(ITestOutputHelper outp) : base(outp)
        {
        }

        [Fact]
        public void KnownTreeDepthForward()
        {
            // TODO
        }
    }


    public class StdTestsForward : StdTestsBase
    {
        public StdTestsForward(ITestOutputHelper outp) : base(outp) {}
        
        protected override ISolver CreateSolver(SolverCommand cmd) 
            => new SingleThreadedForwardSolver(cmd, new SolverNodePoolingFactoryDefault());
        
    }
    
    public class StdTestsReverse : StdTestsBase
    {
        public StdTestsReverse(ITestOutputHelper outp) : base(outp) {}
        
        protected override ISolver CreateSolver(SolverCommand cmd) 
            => new SingleThreadedReverseSolver(cmd, new SolverNodePoolingFactoryDefault());
        
    }
    
    public class StdTestsForwardReverse : StdTestsBase
    {
        public StdTestsForwardReverse(ITestOutputHelper outp) : base(outp) {}
        
        protected override ISolver CreateSolver(SolverCommand cmd) 
            => new SingleThreadedForwardReverseSolver(cmd,new SolverNodePoolingFactoryDefault());
        
    }
    
    public class StdTestsMultiForwardReverse : StdTestsMultiBase
    {
        public StdTestsMultiForwardReverse(ITestOutputHelper outp) : base(outp) {}

        protected override ISolver CreateSolver(SolverCommand cmd) 
            => new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault());
        
    }
    
    public class StdTestsMultiForward : StdTestsMultiBase
    {
        public StdTestsMultiForward(ITestOutputHelper outp) : base(outp) {}

        protected override ISolver CreateSolver(SolverCommand cmd) 
            => new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault())
            {
                ThreadCountReverse = 0,
                ThreadCountForward = 4
            };
        
    }
    
    public class StdTestsMultiReverse : StdTestsMultiBase
    {
        public StdTestsMultiReverse(ITestOutputHelper outp) : base(outp) {}

        protected override ISolver CreateSolver(SolverCommand cmd) 
            => new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault())
            {
                ThreadCountReverse = 4,
                ThreadCountForward = 0
            };
        
    }
}