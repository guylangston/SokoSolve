using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Solver.Lookup;

namespace SokoSolve.Core.Solver
{
    /// <summary>
    ///     This is more useful for testing
    /// </summary>
    public class SingleThreadedForwardReverseSolver : SolverBase<SolverStateForwardReverse>
    {
        private readonly ISolverNodePoolingFactory nodePoolingFactory;
        public SingleThreadedForwardReverseSolver(ISolverNodePoolingFactory nodePoolingFactory) 
            : base(2, 0, "Single Threaded Forward<->Reverse Solver")
        {
            this.nodePoolingFactory = nodePoolingFactory;
        }
        
        
        public override ExitResult Solve(SolverState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            
            var ss = (SolverStateForwardReverse)state;
            if (ss.Forward == null) throw new ArgumentNullException(nameof(ss.Forward));
            if (ss.Reverse == null) throw new ArgumentNullException(nameof(ss.Reverse));
            
            const int tick = 1000;
            
            state.GlobalStats.TotalNodes = 0;
            while (true)
            {
                if (!DequeueAndEval(ss, (TreeState)ss.Forward))
                {
                    return state.Exit;
                }
                
                if (!DequeueAndEval(ss, (TreeState)ss.Reverse))
                {
                    return state.Exit;
                }

                if (state.GlobalStats.TotalNodes % tick == 0 && CheckExit(ss))
                {
                    return state.Exit;
                }
            }
        }
        
        public override SolverStateForwardReverse Init(SolverCommand command)
        {
            var fwdEval  = new ForwardEvaluator(command, nodePoolingFactory);
            var fwdQueue = new SolverQueue();
            var fwdRoot  = fwdEval.Init(command.Puzzle, fwdQueue);
            var fwdPool  = new NodeLookupSimpleList();
            fwdPool.Add(fwdRoot.Recurse().ToList());
            var fwdTree = new TreeState(fwdRoot, fwdPool, fwdQueue, fwdEval);
            
            
            var revEval  = new ReverseEvaluator(command, nodePoolingFactory);
            var revQueue = new SolverQueue();
            var revRoot  = revEval.Init(command.Puzzle, revQueue);
            var revPool  = new NodeLookupSimpleList();
            revPool.Add(revRoot.Recurse().ToList());
            var revTree = new TreeState(revRoot, revPool, revQueue, revEval);

            var state = new SolverStateForwardReverse(command, this, fwdTree, revTree);

            InitState(command, state);
            
            state.Statistics.AddRange(new[]
            {
                state.GlobalStats,
                state.Forward.Queue.Statistics,
                state.Reverse.Queue.Statistics
            });

            return state;
        }
        
        
        protected override ExitResult SolveInner(SolverStateForwardReverse state, TreeState tree)
        {
            throw new NotSupportedException();
        }

        private bool CheckExit(SolverStateForwardReverse state)
        {
            var check = state.Command.ExitConditions.ShouldExit(state);
            if (check != ExitResult.Continue)
            {
                state.EarlyExit = true;
                state.GlobalStats.Completed = DateTime.Now;
                state.Exit = check;
                
                return true;
            }

            return false;
        }

        private bool DequeueAndEval(SolverStateForwardReverse owner, TreeState tree )
        {
            var node = tree.Queue.Dequeue();
            if (node == null)
            {
                owner.Exit = ExitResult.ExhaustedTree;
                return false;
            };

            if (tree.Evaluator.Evaluate(owner, tree, node))
            {
                // Solution
                if (owner.Command.ExitConditions.StopOnSolution)
                    return false;
            }
                
            owner.GlobalStats.TotalNodes++;
            return true;
        }

       

        
    }
}