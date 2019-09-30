using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using SokoSolve.Core.Common;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.PuzzleLogic;

namespace SokoSolve.Core.Solver
{
    public class ReverseEvaluator : INodeEvaluator
    {

        public SolverNode Init(Puzzle puzzle, ISolverQueue queue)
        {
            var solution = puzzle.ToMap(puzzle.Definition.AllGoals);  // START with a solution
            var walls = puzzle.ToMap(puzzle.Definition.Wall);
            // The is only one start, but MANY end soutions. Hence a single root is not a valid representation
            // We use a placeholder node (not an actualy move to hold solutions)
            var root = new SingleThreadedReverseSolver.SyntheticReverseNode()
            {
                CrateMap = puzzle.ToMap(puzzle.Definition.AllGoals),
                MoveMap = new Bitmap(puzzle.Width, puzzle.Height)
            };

            foreach (var crateBefore in solution.TruePositions())
            {
                foreach (var dir in VectorInt2.Directions)
                {
                    // ss
                    var posPlayer = crateBefore + dir;
                    var posPlayerAfter = posPlayer + dir;
                    var crateAfter = crateBefore + dir;

                    // Remember: In reverse mode goals are crates, and crate may be floor
                    if ((puzzle[posPlayer] == puzzle.Definition.Floor 
                        || puzzle[posPlayer] == puzzle.Definition.Player
                        || puzzle[posPlayer] == puzzle.Definition.Crate
                        )
                        &&
                        (puzzle[posPlayerAfter] == puzzle.Definition.Floor
                        || puzzle[posPlayerAfter] == puzzle.Definition.Player
                        || puzzle[posPlayer] == puzzle.Definition.Crate
                        )
                        )
                    {
                        
                            var crate = new Bitmap(solution);
                            crate[crateBefore] = false;
                            crate[crateAfter] = true;
                            
                            var node = new SolverNode()
                            {
                                CrateBefore = crateBefore,
                                CrateAfter = crateAfter,

                                PlayerBefore = posPlayer,
                                PlayerAfter = posPlayerAfter,

                                CrateMap = crate,
                                MoveMap = FloodFill.Fill(walls.BitwiseOR(crate), posPlayerAfter)
                            };

                            if (node.MoveMap.Count > 0)
                            {
                                root.Add(node);
                                queue.Enqueue(node);
                            }

                        
                    }
                }
            }
            return root;
        }


        public bool IsDebugMode { get; set; }

        public bool Evaluate(SolverCommandResult state, ISolverQueue queue, ISolverNodeLookup myPool, ISolverNodeLookup solutionPool, SolverNode node)
        {
            if (node.HasChildren) throw new InvalidOperationException();

            node.Status = SolverNodeStatus.Evaluting;

            var solution = false;
            var toEnqueue = new List<SolverNode>();

            foreach (var move in node.MoveMap.TruePositions())
            {
                foreach (var dir in VectorInt2.Directions)
                {
                    var p = move;
                    var pc = p + dir;
                    var pp = p - dir;
                    if (node.CrateMap[pc]) // crate to push
                    {
                        if (state.StaticMaps.FloorMap[pp] && !node.CrateMap[p])
                        {
                            if (!CheckDeadReverse(state, pp))
                            {
                                var newKid = new SolverNode
                                {
                                    PlayerBefore = p,
                                    PlayerAfter = pp,

                                    CrateBefore = pc,
                                    CrateAfter = p,

                                    CrateMap = new Bitmap(node.CrateMap),
                                    Evaluator = this,
                                };
                                newKid.CrateMap[pc] = false;
                                newKid.CrateMap[p] = true;

                                var boundry = state.StaticMaps.WallMap.BitwiseOR(newKid.CrateMap);
                                newKid.MoveMap = FloodFill.Fill(boundry, pp);

                                newKid.Goals = newKid.CrateMap.BitwiseAND(state.StaticMaps.GoalMap).Count();

                                // Optimisation: PreCalc hash
                                newKid.EnsureHash();
                             
                                // Cycle Check: Does this node exist already?
                                var dup = myPool.FindMatch(newKid);
                                if (dup != null)
                                {
                                    // NOTE: newKid is NOT added as a ChildNode (which means less memory usage)

                                    // Duplicate
                                    newKid.Status = SolverNodeStatus.Duplicate;
                                    state.Statistics.Duplicates++;

                                    if (IsDebugMode)
                                    {
                                        node.AddDuplicate(dup);
                                    }
                                }
                                else
                                {

                                    SolverNode match = null;
                                    if (solutionPool != null) match = solutionPool.FindMatch(newKid);

                                    if (match != null)
                                    {
                                        // Add to tree / itterator
                                        node.Add(newKid);

                                        // Solution
                                        if (state.SolutionsWithReverse == null) state.SolutionsWithReverse = new List<SolutionChain>();
                                        var pair = new SolutionChain()
                                        {
                                            ForwardNode = match,
                                            ReverseNode = newKid,
                                            FoundUsing = this
                                        };
                                        state.SolutionsWithReverse.Add(pair);
                                        solution = true;
                                        state.Command.Debug.Raise(this, SolverDebug.Solution, pair);

                                        foreach (var n in newKid.PathToRoot().Union(match.PathToRoot()))
                                        {
                                            n.Status = SolverNodeStatus.SolutionPath;
                                        }
                                        newKid.Status = SolverNodeStatus.Solution;
                                        match.Status = SolverNodeStatus.Solution;
                                        if (state.Command.ExitConditions.StopOnSolution)
                                        {
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        // Add to tree / itterator
                                        node.Add(newKid);

                                        if (DeadMapAnalysis.DynamicCheck(state.StaticMaps, node))
                                        {
                                            newKid.Status = SolverNodeStatus.Dead;
                                        }
                                        else
                                        {
                                            toEnqueue.Add(newKid);
                                            if (newKid.CrateMap.BitwiseAND(state.StaticMaps.CrateStart).Equals(newKid.CrateMap))
                                            {
                                                // Possible Solution: Did we start in a valid position
                                                if (CheckValidSolutions(state, newKid))
                                                {
                                                    state.Solutions.Add(newKid);
                                                    state.Command.Debug.Raise(this, SolverDebug.Solution, newKid);
                                                    solution = true;

                                                    foreach (var n in newKid.PathToRoot())
                                                    {
                                                        n.Status = SolverNodeStatus.SolutionPath;
                                                    }
                                                    newKid.Status = SolverNodeStatus.Solution;
                                                }
                                                else
                                                {
                                                    newKid.Status = SolverNodeStatus.InvalidSolution;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            node.Status = node.HasChildren ? SolverNodeStatus.Evaluted : SolverNodeStatus.Dead;
            if (node.Status == SolverNodeStatus.Dead  && node.Parent != null)
            {
                var p = (SolverNode)node.Parent;
                p.CheckDead();
            }

            queue.Enqueue(toEnqueue);
            myPool.Add(toEnqueue);

            return solution;
        }

        private bool CheckValidSolutions(SolverCommandResult state, SolverNode posibleSolution)
        {
            var b = state.StaticMaps.WallMap.BitwiseOR(state.StaticMaps.CrateStart);
            var f = state.Command.Puzzle.Player.Position;

            var path = posibleSolution.PathToRoot();
            path.Reverse();
            var start = path[0];
            var t = start.PlayerAfter;
            var first = PathFinder.Find(b, f, t);
            return first != null;
        }

        private bool CheckDeadReverse(SolverCommandResult state, VectorInt2 ppp)
        {
            return false;
            return state.StaticMaps.DeadMap[ppp];
        }
    }
}