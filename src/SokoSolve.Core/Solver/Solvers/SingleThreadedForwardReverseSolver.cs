using System;
using System.Linq;
using SokoSolve.Core.Solver.Lookup;
using SokoSolve.Core.Solver.Queue;

namespace SokoSolve.Core.Solver.Solvers
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
            var fwdPool  = new NodeLookupSimpleList();
            var fwdQueue = new SolverQueue();
            var fwdEval = new ForwardEvaluator(command, nodePoolingFactory);
            var fwdRoot = fwdEval.Init(command, fwdQueue, fwdPool);
            
            fwdPool.Add(fwdRoot.Recurse().ToList());
            var fwdTree = new TreeState(fwdRoot, fwdPool, fwdQueue, fwdEval);
            
            
            var revPool  = new NodeLookupSimpleList();
            var revQueue = new SolverQueue();
            var revEval  = new ReverseEvaluator(command, nodePoolingFactory);
            var revRoot  = revEval.Init(command, revQueue, revPool);
            
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