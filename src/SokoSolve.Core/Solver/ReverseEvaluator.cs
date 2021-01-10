﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;
using Path=SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    public class ReverseEvaluator : NodeEvaluator
    {
        public ReverseEvaluator(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory) : base(cmd, nodePoolingFactory)
        {
        }

        public class SolverNodeRootReverse : SolverNodeRoot
        {
            public SolverNodeRootReverse(VectorInt2 playerBefore, VectorInt2 push, Bitmap crateMap, Bitmap moveMap, INodeEvaluator evaluator, Puzzle puzzle) : base(playerBefore, push, crateMap, moveMap, evaluator, puzzle)
            {
            }
        }

        public override  SolverNode Init(Puzzle puzzle, ISolverQueue queue)
        {
            var solution = puzzle.ToMap(puzzle.Definition.AllGoals); // START with a solution
            var walls = puzzle.ToMap(puzzle.Definition.Wall);
            // The is only one start, but MANY end solutions. Hence a single root is not a valid representation
            // We use a placeholder node (not an actually move to hold solutions)
            var root = new SolverNodeRootReverse(
                new VectorInt2(), new VectorInt2(),
                puzzle.ToMap(puzzle.Definition.AllGoals),
                new Bitmap(puzzle.Width, puzzle.Height),
                this,
                puzzle
                );

            foreach (var crateBefore in solution.TruePositions())
            {
                foreach (var dir in VectorInt2.Directions)
                {
                    //          p0 p1 p2
                    // BEFORE: [.][P][G]  +  <--
                    // AFTER:  [P][G][.]

                    var p0 = crateBefore + dir + dir;
                    var p1 = crateBefore + dir;
                    var p2 = crateBefore;
                    
                    var posPlayer      = crateBefore + dir;
                    var posPlayerAfter = crateBefore + dir + dir;
                    var crateAfter     = crateBefore + dir;

                    bool IsNotGoalAndFloor(VectorInt2 xx) => !puzzle[xx].IsGoal && puzzle[xx].IsFloor;
                    
                    if (puzzle.Area.Contains(p0) && IsNotGoalAndFloor(p0) && IsNotGoalAndFloor(p1))
                    {
                        var kid = nodePoolingFactory.CreateFromPull(root, solution, walls, crateBefore, crateAfter, posPlayerAfter);
                        if (kid.MoveMap.Count > 0 && !root.Children.Contains(kid))
                        {
                            root.Add(kid);
                            queue.Enqueue(kid);
                        }
                    }
                }
            }

            if (!root.HasChildren)
            {
                throw new Exception("Root should always have children");
            }

            return root;
        }
        
        

        protected override bool GenerateChildNodes(SolverState state, TreeStateCore tree, SolverNode node)
        {
            var solution = false;
            foreach (var move in node.MoveMap.TruePositions())
            foreach (var dir in VectorInt2.Directions)
            {
                var p  = move;
                var pc = p + dir;
                var pp = p - dir;
                if (node.CrateMap[pc]// crate to push
                    && state.StaticMaps.FloorMap[pp] && !node.CrateMap[p]
                    && !CheckDeadReverse(state, pp))
                {
                    if (EvaluateValidPull(state, tree, 
                        node, pc, p, pp))
                    {
                        solution = true;
                        if (state.Command.ExitConditions.StopOnSolution)
                        {
                            return true;// Are we ok to skip code below?    
                        }
                    }
                }
            }

            return solution;
        }
        

        private bool EvaluateValidPull(
            SolverState state,
            TreeStateCore tree,
            SolverNode          node,
            VectorInt2          pc,
            VectorInt2          p,
            VectorInt2          pp)
        {
            state.GlobalStats.TotalNodes++;

            var newKid = nodePoolingFactory.CreateFromPull(node, node.CrateMap, state.StaticMaps.WallMap, pc, p, pp);
            
            if (state.Command.Inspector != null && state.Command.Inspector(newKid))
            {
                state.Command.Report?.WriteLine(newKid.ToString());
            }

            // Cycle Check: Does this node exist already?
            var dup = base.FindMatch(state, tree, newKid);
            if (dup is not null)
            {
                // Duplicate
                newKid.Status = SolverNodeStatus.Duplicate;
                state.GlobalStats.Duplicates++;

                if (state.Command.DuplicateMode == DuplicateMode.AddAsChild)
                {
                    toKids.Add(newKid);
                    if (newKid is ISolverNodeDuplicateLink dupLink) dupLink.Duplicate = dup;
                }
                else if (state.Command.DuplicateMode == DuplicateMode.ReuseInPool)
                {
                    nodePoolingFactory.ReturnInstance(newKid); // Add to pool for later re-use?
                    
                }
                else // DuplicateMode.Discard
                {
                    
                }
            }
            else
            {
                // These two should always be the same
                toKids.Add(newKid);

                // If there is a reverse solver, checks its pool for a match, hence a Forward <-> Reverse chain, hence a solution
                var match = tree.Alt?.FindMatch(newKid);
                if (match != null)
                {
                    // Possible Solution: It may be a complete chain; but the chain may have the player on the wrong side // Possible Solution: It may be a complete chain; but the chain may have the player on the wrong side
                    if (CheckAndBuildSolutionChain((SolverStateDoubleTree)state, newKid, match))
                    {
                        return true;
                    }
                }
                else
                {
                    if (DeadMapAnalysis.DynamicCheck(state.StaticMaps, node))
                    {
                        newKid.Status = SolverNodeStatus.Dead;
                    }
                    else
                    {
                        toEnqueue.Add(newKid);
                        if (newKid.IsSolutionReverse(state.StaticMaps))
                        {
                            if (CheckAndBuildSolution(state, newKid))
                            {
                                return true;
                            }
                            
                        }
                    }
                }
            }

            return false;
        }
        
        private bool CheckAndBuildSolution(SolverState state, SolverNode potentialSolution)
        {
            // Possible Solution?
            var path = SolverHelper.ConvertReverseNodeToPath(state.Command.Puzzle, potentialSolution, state.StaticMaps.WallMap, true);
            if (path == null)
            {
                state.Command.Debug?.Raise(this, SolverDebug.FalseSolution, potentialSolution);
                state.GlobalStats.Warnings++;
                return false;
            }

            if (!SolverHelper.CheckSolution(state.Command.Puzzle, path, out var error))
            {
                state.Command.Debug?.RaiseFormat(this, SolverDebug.FalseSolution, "{0} {1}", potentialSolution, error);
                state.GlobalStats.Warnings++;
                return false;
            }
            
            state.Solutions.Add(path);
            state.SolutionsNodes.Add(potentialSolution);

            foreach (var n in potentialSolution.PathToRoot())
            {
                n.Status = SolverNodeStatus.SolutionPath;
            }
            potentialSolution.Status = SolverNodeStatus.Solution;
            
            state.Command.Debug?.Raise(this, SolverDebug.Solution, potentialSolution);
            
            return true;
        }

        private bool CheckAndBuildSolutionChain(SolverStateDoubleTree state, SolverNode revNode, SolverNode fwdNode)
        {
            // Check solution
            var potential = SolverHelper.CheckSolutionChain(state, fwdNode, revNode);
            if (potential != null) 
            {
                foreach (var n in fwdNode.PathToRoot().Union(revNode.PathToRoot()))
                {
                    n.Status = SolverNodeStatus.SolutionPath;
                }
                fwdNode.Status = SolverNodeStatus.Solution;
                revNode.Status = SolverNodeStatus.Solution;
                
                var pair = new SolutionChain
                {
                    ForwardNode = fwdNode,
                    ReverseNode = revNode,
                    FoundUsing  = this,
                    Path        = potential
                };
                
                state.SolutionsChains.Add(pair);
                state.Solutions.Add(potential);

                state.Command.Debug?.Raise(this, SolverDebug.Solution, pair);
                return true;
            }

            return false;
        }

        

        private bool CheckDeadReverse(SolverState state, VectorInt2 ppp)
        {
            return false;  // TODO: How do ReverseDeadMaps work?
            return state.StaticMaps.DeadMap[ppp];
        }
    }
}