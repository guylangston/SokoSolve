using System;
using System.Linq;
using System.Threading.Tasks;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using SokoSolve.Core.Solver.Lookup;
using SokoSolve.Core.Solver.Lookup.Lookup;
using TextRenderZ;
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
        
        public (SolverState a, SolverState b) CompareDepthConsistency(SolverCommand cmd, ISolver a, ISolver b)
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
            
            var rootB = stateB.GetRootForward();
            Assert.NotNull(rootB);
            
            
            var byDepthA = new NodeLookupByDepth(rootA);
            var byDepthB = new NodeLookupByDepth(rootB);
            
            Assert.True(byDepthA.Depth <= byDepthB.Depth);
            
            outp.WriteLine($"A depth {byDepthA.Depth}({stateA.Exit}) vs B depth{byDepthB.Depth}({stateB.Exit})");

            foreach (var item in byDepthA.GetLayers().WithIndex())
            {
                var depthA = (NodeLookupSimpleList)item.item;
                var depthB = (NodeLookupSimpleList)byDepthB[item.index];
                
                var countA = NodeStatusCounts.Count(depthA.GetInnerList());
                var countB = NodeStatusCounts.Count(depthB.GetInnerList());
                
                outp.WriteLine($"Depth: {item.index} :  {countA}/{countA.Total} vs {countB}/{countB.Total} || {depthA.GetInnerList().Count} vs {depthB.GetInnerList().Count}");
                
                if (countA.Open == 0 && countB.Open == 0)
                {
                    if (countA.Total != countB.Total)
                    {
                        outp.WriteLine(countA.ToString());
                        outp.WriteLine(countB.ToString());

                        var comp = new SolverNodeListComparer();
                        var mismatch = comp.Compare(depthA.GetInnerList(), depthB.GetInnerList());

                        if (mismatch > 0)
                        {
                            outp.WriteLine("DupA:");
                            outp.WriteLine(FluentString.Join(comp.DupA, new JoinOptions() { Sep = Environment.NewLine}));
                            outp.WriteLine("DupB:");
                            outp.WriteLine(FluentString.Join(comp.DupB, new JoinOptions() { Sep = Environment.NewLine}));
                            outp.WriteLine("In A Not B:");
                            outp.WriteLine(FluentString.Join(comp.NotInB));
                            outp.WriteLine("In B Not A:");
                            outp.WriteLine(FluentString.Join(comp.NotInA));
                        
                            throw new Exception($"{countA} != {countB} = {mismatch} mismatches");    
                        }
                    }

                    if (countA.ToString() != countB.ToString())
                    {
                        outp.WriteLine(countA.ToString());
                        outp.WriteLine(countB.ToString());

                        var comp     = new SolverNodeListComparer();
                        var mismatch = comp.Compare(depthA.GetInnerList(), depthB.GetInnerList());
                        if (mismatch > 0)
                        {
                            outp.WriteLine("WARNING");
                            outp.WriteLine("DupA:");
                            outp.WriteLine(FluentString.Join(comp.DupA, new JoinOptions() { Sep = Environment.NewLine}));
                            outp.WriteLine("DupB:");
                            outp.WriteLine(FluentString.Join(comp.DupB, new JoinOptions() { Sep = Environment.NewLine}));
                            outp.WriteLine("In A Not B:");
                            outp.WriteLine(FluentString.Join(comp.NotInB));
                            outp.WriteLine("In B Not A:");
                            outp.WriteLine(FluentString.Join(comp.NotInA));    
                        }
                    }
                }

                foreach (var aa in depthA.GetInnerList())
                {
                    var bb = depthB.FindMatch(aa);
                    Assert.NotNull(bb);
                    Assert.True(aa.Equals(bb), $"{aa} != {bb}");
                }
                
                
                // TODO: How to compare?
            }
            
            // Children Equal
            CompareNode(rootA, rootB, 10);

            return (stateA, stateB);
            

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
        
        //[Fact]
        public void SingleVsMulti__Forward()
        {
            var ident  = new PuzzleIdent("SQ1", "DDD");
            var puzzle = Puzzle.Builder.DefaultTestPuzzle();


            var iot = new SolverContainerByType();
            var cmd = new SolverCommand(puzzle, ident, new ExitConditions()
            {
                StopOnSolution = false,
                Duration       = TimeSpan.FromSeconds(20),
                TotalNodes     = 20_000,
            }, iot)
            {
                //SafeMode = SafeMode.Throw
            };

            CompareDepthConsistency(cmd,
                new SingleThreadedForwardSolver(cmd, new SolverNodePoolingFactoryDefault()),
                new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault())
                {
                    ThreadCountReverse = 0,
                    ThreadCountForward = 4
                });
        }
        
        [Fact]
        public void SingleVsMulti_StrictBreadthFirst__Forward()
        {
            var ident  = new PuzzleIdent("SQ1", "DDD");
            var puzzle = Puzzle.Builder.DefaultTestPuzzle();


            var iot = new SolverContainerByType();
            iot.Register<ISolverQueue>(_=> new SolverQueueBreadthFirst());
            
            var cmd = new SolverCommand(puzzle, ident, new ExitConditions()
            {
                StopOnSolution = false,
                Duration       = TimeSpan.FromSeconds(20),
                TotalNodes     = 20_000,
            }, iot)
            {
                //SafeMode = SafeMode.Off
            };

            var (a, b) = CompareDepthConsistency(cmd,
                new SingleThreadedForwardSolver(cmd, new SolverNodePoolingFactoryDefault()),
                new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault())
                {
                    ThreadCountReverse = 0,
                    ThreadCountForward = 4
                });

            var bb = b as MultiThreadedSolverState;
            Assert.True(bb.QueueForward is SolverQueueBreadthFirst);
        }
        
         
        [Fact]
        public void SingleVsMultiForcesSingle__Forward()
        {
            var ident  = new PuzzleIdent("SQ1", "DDD");
            var puzzle = Puzzle.Builder.DefaultTestPuzzle();


            var iot = new SolverContainerByType();
            var cmd = new SolverCommand(puzzle, ident, new ExitConditions()
            {
                StopOnSolution = false,
                Duration       = TimeSpan.FromSeconds(20),
                TotalNodes     = 20_000
            }, iot)
            {
               // SafeMode = SafeMode.Throw
            };

            CompareDepthConsistency(cmd,
                new SingleThreadedForwardSolver(cmd, new SolverNodePoolingFactoryDefault()),
                new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault())
                {
                    ThreadCountReverse = 0,
                    ThreadCountForward = 1
                });
        }
        
        
        //[Fact]
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
                Duration       = TimeSpan.FromSeconds(20),
                TotalNodes     = 100_000
            }, iot)
            {
               // SafeMode = SafeMode.Throw
            };

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