using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;
using Path=SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    public class ForwardEvaluator : NodeEvaluator
    {
        public ForwardEvaluator(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory) : base(cmd, nodePoolingFactory)
        {
        }

        public SolverNodeRoot CreateRoot(Puzzle puzzle)
        {
            var crate       = puzzle.ToMap(puzzle.Definition.AllCrates);
            var moveBoundry = crate.BitwiseOR(puzzle.ToMap(puzzle.Definition.Wall));
            var move        = FloodFill.Fill(moveBoundry, puzzle.Player.Position);
            var root = new SolverNodeRoot(
                puzzle.Player.Position, new VectorInt2(),
                crate, move,
                this,
                puzzle
            );
            
            return root;
        }

        public override SolverNode Init(Puzzle puzzle, ISolverQueue queue)
        {
            var root = CreateRoot(puzzle);
            queue.Enqueue(root);
            return root;
        }
        

        // NOTE: One Evaluator per Thread! Below is an optimisation, stopping reallocation per node
        readonly List<SolverNode> toKids    = new List<SolverNode>();
        readonly List<SolverNode> toEnqueue = new List<SolverNode>();
        
        public override bool Evaluate(SolverStateEvaluation state, SolverNode node)
        {
            if (node.HasChildren) throw new InvalidOperationException();

            if (state.Command.SafeMode != SafeMode.Off)
            {
                lock (node)
                {
                    return EvaluateInner(state, node);    
                }
            }
            else
            {
                return EvaluateInner(state,  node);    
            }
        }
        
     
        private bool EvaluateInner(SolverStateEvaluation state, SolverNode node)
        {
            node.Status = SolverNodeStatus.InProgress;
            toKids.Clear();
            toEnqueue.Clear();
            

            // We evaluate all kids as a batch in this single call (and thread)
            var solution = false;
            foreach (var move in node.MoveMap.TruePositions())
            {
                foreach (var dir in VectorInt2.Directions)
                {
                    var p   = move;
                    var pp  = p + dir;
                    var ppp = pp + dir;
                    if (node.CrateMap[pp]                                        // crate to push
                        && state.StaticMaps.FloorMap[ppp] && !node.CrateMap[ppp] // into free space?
                        && !state.StaticMaps.DeadMap[ppp])                       // Valid Push
                    {
                        if (EvaluateValidPush(state, 
                            state.Pool, state.PoolAlt, 
                            node, 
                            pp, ppp, p, dir))
                        {
                            solution = true;
                            if (state.Command.ExitConditions.StopOnSolution)
                            {
                                return true;// Are we ok to skip code below?    
                            }
                        }
                    }
                }
            }
            
            // Done
            node.Status = SolverNodeStatus.Evaluated;
            state.Pool.Add(node);

            if (toKids.Any())
            {
                node.SetChildren(toKids);
                state.Queue.Enqueue(toEnqueue);

                state.GlobalStats.TotalDead += node.CheckDead();// Children may be evaluated as dead already
                
                return solution;
            }
            else
            {
                // No kids means it cannot be a solution/solutionpath
                node.Status = SolverNodeStatus.Dead;
                state.GlobalStats.TotalDead++;
                if (node.Parent != null)
                {
                    state.GlobalStats.TotalDead += node.Parent.CheckDead();
                }

                return false;
            }
        }



        // Should never add to state (tree, pool, queue) rather it should add to temp arrays
        private bool EvaluateValidPush(SolverState state,
            INodeLookupReadOnly pool,
            INodeLookupReadOnly? reversePool,
            SolverNode          node,
            VectorInt2          pp,
            VectorInt2          ppp,
            VectorInt2          p,
            VectorInt2          push)
        {
            state.GlobalStats.TotalNodes++;
            
            var newKid = nodePoolingFactory.CreateFromPush(node, node.CrateMap, state.StaticMaps.WallMap, p, pp, ppp, push);

            if (state.Command.Inspector != null && state.Command.Inspector(newKid))
            {
                state.Command.Report?.WriteLine(newKid.ToString());
            }

            // Cycle Check: Does this node exist already?
            var dup = pool.FindMatch(newKid);
            if (dup == null && state.Command.SafeMode != SafeMode.Off)  // Double-check?
            {
                dup = ConfirmDupLookup(state, pool, node, newKid);  // Fix or Throw
            }
            
            if (dup != null)
            {
                if (object.ReferenceEquals(dup, newKid)) throw new InvalidDataException();
                if (dup.SolverNodeId == newKid.SolverNodeId) throw new InvalidDataException();
                
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
                var match = reversePool?.FindMatch(newKid);
                if (match != null)
                {
                    // Possible Solution: It may be a complete chain; but the chain may have the player on the wrong side
                    if (NewSolutionChain(state, newKid, match))
                    {
                        return true;
                    }
                    else
                    {
                        state.GlobalStats.Warnings++;
                        state.Command.Debug?.Raise(this, SolverDebug.FalseSolution, new SolutionChain()
                        {
                            ForwardNode = match,
                            ReverseNode = newKid
                        });
                    }
                }
                else
                {
                    if (DeadMapAnalysis.DynamicCheck(state.StaticMaps, node))
                    {
                        newKid.Status = SolverNodeStatus.Dead;
                        state.GlobalStats.TotalDead++;
                    }
                    else
                    {
                        toEnqueue.Add(newKid);

                        if (newKid.IsSolutionForward(state.StaticMaps))
                        {
                            var path = SolverHelper.ConvertForwardNodeToPath(newKid, state.StaticMaps.WallMap);
                            if (path == null)
                            {
                                state.GlobalStats.Warnings++;
                                state.Command.Debug?.Raise(this, SolverDebug.FalseSolution, newKid);
                            }
                            else
                            {
                                // Solution
                                state.SolutionsNodes ??= new List<SolverNode>();
                                state.SolutionsNodes.Add(newKid);

                                state.Solutions ??= new List<Path>();
                                state.Solutions.Add(path);
                                
                                state.Command.Debug?.Raise(this, SolverDebug.Solution, newKid);
                                
                                foreach (var n in newKid.PathToRoot())
                                    n.Status = SolverNodeStatus.SolutionPath;
                                newKid.Status = SolverNodeStatus.Solution;

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

     

        private bool NewSolutionChain(SolverState state, SolverNode fwdNode, SolverNode revNode)
        {
            state.SolutionsChains ??= new List<SolutionChain>();
            
            // Check solution
            var potential = SolverHelper.CheckSolutionChain(state, fwdNode, revNode); 
            if (potential != null)
            {
                foreach (var n in fwdNode.PathToRoot().Union(revNode.PathToRoot()))
                    n.Status = SolverNodeStatus.SolutionPath;
                fwdNode.Status = SolverNodeStatus.Solution;
                revNode.Status = SolverNodeStatus.Solution;
                
                var pair = new SolutionChain
                {
                    ForwardNode = fwdNode,
                    ReverseNode = revNode,
                    FoundUsing  = this,
                    Path = potential
                };
                state.SolutionsChains.Add(pair);

                state.Solutions ??= new List<Path>();
                state.Solutions.Add(potential);

                state.Command.Debug?.Raise(this, SolverDebug.Solution, pair);
                return true;
            }

            return false;
        }
    }
}