﻿using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Debugger;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using Xunit;
using Xunit.Abstractions;
using Console = System.Console;

namespace SokoSolve.Tests.SolverTests
{

    public class SingleThreadedForwardReverseSolverTests
    {
        private readonly ITestOutputHelper outp;

        public SingleThreadedForwardReverseSolverTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        private SolverState PerformStandardTest(
            Puzzle puzzle,
            ExitConditions? exit = null, 
            Action<SolverState>? checkAfterInit = null,
            Func<SolverNode, bool>? inspector = null,
            IDebugEventPublisher? debugger = null
            )
        {
            exit ??= new ExitConditions
            {
                Duration       = TimeSpan.FromSeconds(60),
                StopOnSolution = true,
                TotalNodes     = int.MaxValue,
                TotalDead      = int.MaxValue
            };
            // arrange
            var solver = new SingleThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault());
            var command = new SolverCommand( puzzle.Clone(), PuzzleIdent.Temp(),  exit, SolverContainerByType.DefaultEmpty)
            {
                Report = new XUnitOutput(outp),
                Inspector = node =>
                {
                    if (inspector != null && inspector(node))
                    {
                        return true;
                    }
                    return false;
                },
                Debug = debugger
            };

            // act 
            var result = solver.Init(command);
            if (checkAfterInit != null)
            {
                checkAfterInit(result);
            }
            solver.Solve(result);
            Console.WriteLine(result.ExitDescription);
            Console.WriteLine(SolverHelper.GenerateSummary(result));
            result.ThrowErrors();

            // assert    
            Assert.NotNull(result);

            Assert.True(result.HasSolution, "Must Have Solution");
            Assert.NotNull(result.Solutions);
            Assert.NotEmpty(result.Solutions);

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

        [Fact]
        public void T002_DefaultTest_HasSolutions()
        {
            var res = PerformStandardTest(Puzzle.Builder.DefaultTestPuzzle(),
                debugger: new FuncDebugEventPublisher(
                    (e) => {
                        outp.WriteLine(e.ToString());
                    },
                    (ee) => {

                        outp.WriteLine(ee.ctx[2].ToString());
                        throw new Exception("Should never happen");


                    }));

            Assert.True(res.HasSolution);
        }

        [Fact]
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

        // Local dev only -- scratchpad
        //[Fact]
        public void SQ1_17()
        {
            var puzzle = Puzzle.Builder.FromLines(new[] {
                "#########",
                "#O.O....#",
                "#OXO.O..#",
                "##.###P.#",
                "~#..X..##",
                "~#.XX.##~",
                "~#..X.#~~",
                "~#..###~~",
                "~####~~~~",
            });
            var res = PerformStandardTest(puzzle);
        }
    }
}