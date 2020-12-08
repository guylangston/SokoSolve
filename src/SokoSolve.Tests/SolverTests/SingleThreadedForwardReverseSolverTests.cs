using System;
using System.Reflection.Metadata.Ecma335;
using SokoSolve.Core;
using SokoSolve.Core.Common;
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
            ExitConditions exit = null,
            Func<SolverNode, bool>? inspector = null
            )
        {
            exit = exit ?? new ExitConditions
            {
                Duration = TimeSpan.FromSeconds(60),
                StopOnSolution = true,
                TotalNodes = int.MaxValue,
                TotalDead = int.MaxValue
            };
            // arrange
            var solver = new SingleThreadedForwardReverseSolver(new SolverNodeFactoryTrivial());
            var command = new SolverCommand
            {
                Puzzle = puzzle.Clone(),
                Report = new XUnitOutput(outp),
                ExitConditions = exit,
                Inspector = inspector
            };

            // act 
            var result = solver.Init(command);
            solver.Solve(result);
            Console.WriteLine(result.ExitDescription);
            Console.WriteLine(SolverHelper.GenerateSummary(result));
            result.ThrowErrors();

            // assert    
            Assert.NotNull(result);

            Assert.True(result.HasSolution);
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
        public void T002_BaseLine()
        {
            var res = PerformStandardTest(Puzzle.Builder.DefaultTestPuzzle());

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
            var res = PerformStandardTest(puzzle, null, x =>
            {
                if (x.GetHashCode() == 122665)
                {
                    outp.WriteLine(x.ToString());
                    return true;
                }

                return false;
            });
        }
    }
}