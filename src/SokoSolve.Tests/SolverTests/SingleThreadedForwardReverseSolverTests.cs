using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
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
            var solver = new SingleThreadedForwardReverseSolver(command, new SolverNodePoolingFactoryDefault());

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

        [Fact]
        public void T004_ConsistentDepth()
        {
            var ident = new PuzzleIdent("SQ1", "P5");
            var puzzle = Puzzle.Builder.FromLines(new[] {
                "~~~~~~~~~~~#####",
                "~~~~~~~~~~##...#",
                "~~~~~~~~~~#....#",
                "~~~~####~~#.X.##",
                "~~~~#..####X.X#~",
                "~~~~#.....X.X.#~",
                "~~~##.##.X.X.X#~",
                "~~~#..O#..X.X.#~",
                "~~~#..O#......#~",
                "#####.#########~",
                "#OOOO.P..#~~~~~~",
                "#OOOO....#~~~~~~",
                "##..######~~~~~~",
                "~####~~~~~~~~~~~",
            });


            var iot = new SolverContainerByType();
            var cmd = new SolverCommand(puzzle, ident, new ExitConditions()
            {
                StopOnSolution = false,
                Duration       = TimeSpan.FromSeconds(20),
                TotalNodes     = 20000
            }, iot);
            var f      = new SingleThreadedForwardSolver(cmd, new SolverNodePoolingFactoryDefault());
            var stateF = f.Init(cmd);
            
            var m = new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault())
            {
                ThreadCountReverse = 0,
                ThreadCountForward = 4
            };
            var stateM = m.Init(cmd);


            Task.WaitAll(
                Task.Run(() => f.Solve(stateF)),
                Task.Run(() => m.Solve(stateM))
                );
            
            // Compare
            Assert.Equal(ExitResult.Continue, stateF.Exit);
            Assert.Equal(ExitResult.Continue, stateM.Exit);

            var rootF = stateF.GetRootForward();
            Assert.NotNull(rootF);
            
            var rootM = stateM.GetRootForward();
            Assert.NotNull(rootM);
            
            
            // Children Equal
            CompareNode(rootF, rootM, 10);




        }
        
        
        private void CompareNode(SolverNode a, SolverNode b, int maxDepth)
        {
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.Equal(a.CrateMap, b.CrateMap);
            Assert.Equal(a.MoveMap, b.MoveMap);

            var aa = a.Children.ToArray();
            var bb = b.Children.ToArray();

            if (a.Status != SolverNodeStatus.UnEval)
            {
                if (aa.Length != bb.Length)
                {
                    outp.WriteLine($"A : {a}");
                    outp.WriteLine(a.ToStringMaps());
                    foreach (var temp in aa)
                    {
                        outp.WriteLine(temp.ToString());
                    }

                    outp.WriteLine($"B : {b}");
                    outp.WriteLine(b.ToStringMaps());
                    foreach (var temp in bb)
                    {
                        outp.WriteLine(temp.ToString());
                    }

                    throw new Exception("Children counts incorrect");
                }
                
                foreach (var node in aa)
                {
                    var exists = bb.First(x =>x.GetHashCode() == node.GetHashCode() &&  x.Equals(node));

                    if (exists.GetDepth() <= maxDepth)
                    {
                        CompareNode(node, exists, maxDepth); // recurse    
                    }
                
                }
            }

            

            
            
            
        }
    }
}