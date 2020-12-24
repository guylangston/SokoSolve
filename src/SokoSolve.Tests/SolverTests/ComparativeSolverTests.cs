using System;
using System.Linq;
using System.Threading.Tasks;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using SokoSolve.Core.Solver.Lookup;
using SokoSolve.Core.Solver.Lookup.Lookup;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests.SolverTests
{
    public class ComparativeSolverTests
    {
        private readonly ITestOutputHelper outp;
        
        public ComparativeSolverTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }
        
        public void CompareDepthConsistency(SolverCommand cmd, ISolver a, ISolver b)
        {
            var stateA = a.Init(cmd);
            var stateB = b.Init(cmd);


            Task.WaitAll(
                Task.Run(() => a.Solve(stateA)),
                Task.Run(() => b.Solve(stateB))
            );
            
            // Compare
            var rootA = stateA.GetRootForward();
            Assert.NotNull(rootA);
            
            var rootB = stateA.GetRootForward();
            Assert.NotNull(rootB);
            
            // Children Equal
            CompareNode(rootA, rootB, 100);


            var byDepthA = new NodeLookupByDepth(rootA);
            var byDepthB = new NodeLookupByDepth(rootB);
            
            Assert.Equal(byDepthA.Depth, byDepthB.Depth);

            foreach (var item in byDepthA.GetLayers().WithIndex())
            {
                var depthA = (NodeLookupSimpleList)item.item;
                var depthB = (NodeLookupSimpleList)byDepthB[item.index];
                
                Assert.Equal(depthA.GetInnerList(), depthB.GetInnerList());
                
                // TODO: How to compare?
            }
            

            void CompareNode(SolverNode a, SolverNode b, int maxDepth)
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
                        var exists = bb.First(x => x.GetHashCode() == node.GetHashCode() && x.Equals(node));

                        if (exists.GetDepth() <= maxDepth)
                        {
                            CompareNode(node, exists, maxDepth);// recurse    
                        }

                    }
                }
            }
        }
        
        [Fact]
        public void SingleVsMulti__Forward()
        {
            var ident  = new PuzzleIdent("SQ1", "DDD");
            var puzzle = Puzzle.Builder.DefaultTestPuzzle();


            var iot = new SolverContainerByType();
            var cmd = new SolverCommand(puzzle, ident, new ExitConditions()
            {
                StopOnSolution = false,
                Duration       = TimeSpan.FromSeconds(10),
                TotalNodes     = 10_000
            }, iot);

            CompareDepthConsistency(cmd,
                new SingleThreadedForwardSolver(cmd, new SolverNodePoolingFactoryDefault()),
                new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault())
                {
                    ThreadCountReverse = 0,
                    ThreadCountForward = 4
                });
        }
        
        
        [Fact]
        public void SingleVsMulti__Forward_SP1P5()
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
                Duration       = TimeSpan.FromSeconds(10),
                TotalNodes     = 10_000
            }, iot);

            CompareDepthConsistency(cmd,
                new SingleThreadedForwardSolver(cmd, new SolverNodePoolingFactoryDefault()),
                new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault())
                {
                    ThreadCountReverse = 0,
                    ThreadCountForward = 4
                });
        }
    }
}