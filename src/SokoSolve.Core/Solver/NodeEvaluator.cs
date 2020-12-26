using System;
using System.Collections.Generic;
using TextRenderZ;

namespace SokoSolve.Core.Solver
{
    public abstract class NodeEvaluator : INodeEvaluator
    {
        protected readonly ISolverNodePoolingFactory nodePoolingFactory;

        protected NodeEvaluator(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory)
        {
            this.nodePoolingFactory = nodePoolingFactory;
        }
        
        public abstract SolverNode Init(Puzzle puzzle, ISolverQueue queue);
        public abstract bool       Evaluate(SolverStateEvaluation state, SolverNode node);
        
      
        protected SolverNode? ConfirmDupLookup(SolverState solverState, INodeLookupReadOnly pool, SolverNode node,  SolverNode newKid)
        {
            if (solverState.Command.SafeMode != SafeMode.Off)
            {
                
                  /* SafeMode means:
                                In the fast lock-less implementations, nodes may get added during a lookup; 
                                meaning they will get missed and return null (no match), when actually they should be found 
                             */
                    var doubleCheck = pool.FindMatch(newKid);
                    var root                             = node.Root();
                    foreach (var treeNode in root.Recurse())
                    {
                        if (treeNode.Equals(newKid))
                        {
                            if (object.ReferenceEquals(treeNode, newKid)) throw new Exception("Should not be in tree yet");
                            if (treeNode.CompareTo(newKid) != 0) throw new InvalidOperationException();
                            
                            var shouldExist                      = pool.FindMatch(treeNode);
                            
                            var wasNotFoundOnFirstAttempt = pool.FindMatch(newKid);
                            if (wasNotFoundOnFirstAttempt != null)
                            {
                                // Must have been added to pool during this call
                                var check1 = object.ReferenceEquals(treeNode, wasNotFoundOnFirstAttempt);
                                var check2 = treeNode.Equals(wasNotFoundOnFirstAttempt);
                            }

                            var sizes = $"Counts(Tree:{root.CountRecursive()}, Pool:{pool.Statistics.TotalNodes})";
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
            return null;
        }
        
        public string TypeDescriptor => $"{GetType().Name}";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => null;
    }
}