using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TextRenderZ;

namespace SokoSolve.Core.Solver
{
    public abstract class NodeEvaluator : BaseComponent, INodeEvaluator
    {
        protected readonly ISolverNodePoolingFactory nodePoolingFactory;

        protected NodeEvaluator(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory)
        {
            this.nodePoolingFactory = nodePoolingFactory;
        }
        
        public abstract SolverNode Init(Puzzle puzzle, ISolverQueue queue);
        protected abstract bool GenerateChildNodes(SolverState state, TreeStateCore tree, SolverNode node);
        
        
        // OPTIMISATION: (Depends on 1 Evaluator per Thread!) Stop 2x array allocations reallocation per node evaluated
        // TODO: It may be safer to rather associate this per state
        readonly protected List<SolverNode> toKids    = new List<SolverNode>();
        readonly protected List<SolverNode> toEnqueue = new List<SolverNode>();

        
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
        
        protected bool EvaluateInner(SolverState state, TreeStateCore tree, SolverNode node)
        {
            node.Status = SolverNodeStatus.InProgress;
            toKids.Clear();
            toEnqueue.Clear();
            
            var solution = GenerateChildNodes(state, tree, node);
            
            // Done
            node.Status = SolverNodeStatus.Evaluated;
            tree.Pool.Add(node);

            if (toKids.Any())
            {
                node.SetChildren(toKids);
                tree.Queue.Enqueue(toEnqueue);

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
        
        
        
        protected SolverNode? FindMatch(SolverState state, TreeStateCore tree, SolverNode newKid)
        {
            var match = tree.FindMatch(newKid);
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
      
        protected SolverNode? ConfirmDupLookup(SolverState solverState, TreeStateCore tree, SolverNode newKid)
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