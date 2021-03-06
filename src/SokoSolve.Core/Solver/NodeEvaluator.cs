using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SokoSolve.Core.Analytics;
using TextRenderZ;

namespace SokoSolve.Core.Solver
{
    public class StrategyTopology
    {
        // 1. Pool [ Eval ], Queue [ UnEval ]
        // 2. Pool [ Eval, UnEval ], Queue is not Lookup
        // 3. Universal Pool

        public bool PoolEval_QueueUnEval { get; private set; }
        public bool PoolEvalUnEval       => !PoolEvalUnEval;
        
        public void UsePoolEval_QueueUnEval() => PoolEval_QueueUnEval = true;
        public void UsePoolEvalUnEval() => PoolEval_QueueUnEval = false;
    }
    
    public abstract class NodeEvaluator : BaseComponent, INodeEvaluator
    {
        protected readonly ISolverNodePoolingFactory nodePoolingFactory;

        protected NodeEvaluator(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory)
        {
            this.nodePoolingFactory = nodePoolingFactory;
        }
        
        protected abstract SolverNode CreateRoot(Puzzle puzzle);
        protected abstract bool GenerateChildNodes(SolverState state, TreeStateCore tree, SolverNode node);
        protected abstract bool CheckAndBuildSingleTreeSolution(SolverState state, SolverNode newKid);
        protected abstract bool CheckAndBuildSolutionChain(SolverStateDoubleTree state, SolverNode fwdNode, SolverNode revNode);
        
        public  SolverNode Init(SolverCommand cmd, ISolverQueue queue, INodeLookup pool)
        {
            var root = CreateRoot(cmd.Puzzle);
            if (root is ReverseEvaluator.SolverNodeRootReverse rev)
            {
                foreach (var kid in root.Children)
                {
                    if (cmd.Topology.PoolEval_QueueUnEval)
                    {
                        queue.Enqueue(kid);
                    }
                    else 
                    {
                        pool.Add(kid);
                        queue.Enqueue(kid);
                    }
                }
            }
            else
            {
                if (cmd.Topology.PoolEval_QueueUnEval)
                {
                    queue.Enqueue(root);
                }
                else 
                {
                    pool.Add(root);
                    queue.Enqueue(root);
                }    
            }
            
            return root;
        }

        // OPTIMISATION: (Depends on 1 Evaluator per Thread!) Stop 2x array allocations reallocation per node evaluated
        readonly List<SolverNode> toKids    = new List<SolverNode>(20);
        readonly List<SolverNode> toEnqueue = new List<SolverNode>(20);
        
        public virtual bool Evaluate(SolverState state, TreeStateCore tree, SolverNode node)
        {
            if (node.HasChildren) throw new InvalidOperationException();

            if (state.Command.SafeMode != SafeMode.Off)
            {
                lock (node)
                {
                    return EvaluateInner(state, tree, node);    
                }
            }
            else
            {
                return EvaluateInner(state, tree, node);    
            }
        }
        
        
        
        bool EvaluateInner(SolverState state, TreeStateCore tree, SolverNode node)
        {
            toKids.Clear();
            toEnqueue.Clear();
            
            node.Status = SolverNodeStatus.InProgress;
            var solution = GenerateChildNodes(state, tree, node);
            node.Status = SolverNodeStatus.Evaluated;
            
            // Set State
            if (state.Command.Topology.PoolEval_QueueUnEval ) // Pool [ Eval ], Queue [ UnEval ] 
            {
                tree.Pool.Add(node);
                if (toEnqueue.Any())
                {
                    //!!! MAJOR CAll: Slow/Expensive/Blocking
                    tree.Queue.Enqueue(toEnqueue);
                }
            }
            else //  Pool [ Eval, UnEval ], Queue is not Lookup
            {
                if (toEnqueue.Any())
                {
                    tree.Pool.Add(toEnqueue);
                    tree.Queue.Enqueue(toEnqueue);    
                }
            }

            if (toKids.Any())
            {
                node.SetChildren(toKids);
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

       
        protected bool EvaluateNewChild(SolverState state, TreeStateCore tree, SolverNode  parent, SolverNode newKid)
        {
            state.GlobalStats.TotalNodes++;

            if (state.Command.Inspector != null && state.Command.Inspector(newKid))
            {
                state.Command.Report?.WriteLine(newKid.ToString());
            }

            // Cycle Check: Does this node exist already?
            var dup = FindMatch(state, tree, newKid);  //!!! MAJOR CAll: Slow/Expensive/Blocking
            if (dup != null)
            {
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
                toKids.Add(newKid); 
                
                // If there is a reverse solver, checks its pool for a match, hence a Forward <-> Reverse chain, hence a solution
                var match = FindMatch(state, tree.Alt, newKid);  //!!! MAJOR CAll: Slow/Expensive/Blocking
                if (match != null)
                {
                    // Possible Solution: It may be a complete chain; but the chain may have the player on the wrong side
                    if (CheckAndBuildSolutionChain((SolverStateDoubleTree)state, newKid, match))
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
                    if (DeadMapAnalysis.DynamicCheck(state.StaticMaps, newKid))
                    {
                        newKid.Status = SolverNodeStatus.Dead;
                        state.GlobalStats.TotalDead++;
                    }
                    else
                    {
                        toEnqueue.Add(newKid);
                        if (CheckAndBuildSingleTreeSolution(state, newKid))
                        {
                            return true;  // SOLUTION!
                        }
                    }
                }
            }

            return false;
        }

        //!!! MAJOR CAll: Slow/Expensive/Blocking
        SolverNode? FindMatch(SolverState state, TreeStateCore? tree, SolverNode newKid)
        {
            if (tree == null) return null;
            
            var match = tree.FindMatch(state, newKid);
            if (match != null)
            {
                if (object.ReferenceEquals(match, newKid)) throw new InvalidDataException();
                if (match.SolverNodeId == newKid.SolverNodeId) throw new InvalidDataException();
                return match;
            }
            
            if (state.Command.SafeMode != SafeMode.Off) 
            {
                match = ConfirmDupLookup(state, tree, newKid);  
            }

            return match;
        }
      
        SolverNode? ConfirmDupLookup(SolverState solverState, TreeStateCore tree, SolverNode newKid)
        {
             /* SafeMode means:
                                In the fast lock-less implementations, nodes may get added during a lookup; 
                                meaning they will get missed and return null (no match), when actually they should be found 
                             */
            var doubleCheck = tree.Pool.FindMatch(newKid);
            var root  = tree.Root;
            foreach (var treeNode in root.Recurse())
            {
                if (treeNode.Equals(newKid))
                {
                    if (object.ReferenceEquals(treeNode, newKid)) throw new Exception("Should not be in tree yet");
                    if (treeNode.CompareTo(newKid) != 0) throw new InvalidOperationException();
                    
                    var shouldExist = tree.Pool.FindMatch(treeNode);
                    
                    var wasNotFoundOnFirstAttempt = tree.Pool.FindMatch(newKid);
                    if (wasNotFoundOnFirstAttempt != null)
                    {
                        // Must have been added to pool during this call
                        var check1 = object.ReferenceEquals(treeNode, wasNotFoundOnFirstAttempt);
                        var check2 = treeNode.Equals(wasNotFoundOnFirstAttempt);
                    }

                    var sizes = $"Counts(Tree:{root.CountRecursive()}, Pool:{tree.Pool.Statistics.TotalNodes})";
                    var message = FluentString.Create()
                                              .Append($"{newKid}").Sep("\n")
                                              .Append($"-> [DUP] doubleCheck:{doubleCheck}").Sep("\n")
                                              .Append($"-> [DUP] treeNode  => {treeNode}").Sep("\n")
                                              .Append($"-> [DUP] {nameof(wasNotFoundOnFirstAttempt)} =>{wasNotFoundOnFirstAttempt}").Sep("\n")
                                              .Append($"-- sizes:{sizes}").Sep("\n")
                                              .ToString();
                        
                    solverState.Command.Report?.WriteLine(message);

                    solverState.GlobalStats.Warnings++;
                    
                    if (solverState.Command.SafeMode == SafeMode.Throw)
                    {
                        throw new Exception(message);    
                    }
                    
                    return treeNode;
                }
            }

            return null;
            
        }

    }
}