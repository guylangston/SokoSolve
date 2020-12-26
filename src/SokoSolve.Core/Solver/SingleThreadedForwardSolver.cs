using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Solver.Lookup;

namespace SokoSolve.Core.Solver
{
    public sealed class SingleThreadedForwardSolver : SolverBase<SolverStateEvaluationSingleThreaded>
    {
        private readonly ISolverNodePoolingFactory nodePoolingFactory;
        
        public SingleThreadedForwardSolver(ISolverNodePoolingFactory nodePoolingFactory)
        {
            this.nodePoolingFactory = nodePoolingFactory;
        }
        
        public override SolverStateEvaluationSingleThreaded InitInner(SolverCommand command)
        {
            var eval  = new ForwardEvaluator(command, nodePoolingFactory);
            var queue = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(() => new SolverQueue());
            var root  = eval.Init(command.Puzzle, queue);
            var pool  = command.ServiceProvider.GetInstanceElseDefault<INodeLookup>(() => new NodeLookupSimpleList());
            pool.Add(root.Recurse().ToArray());

            var state = SolverHelper.Init(
                new SolverStateEvaluationSingleThreaded(
                    command, this, eval,
                    root, pool, queue, null, null), command);
            
        
            state.Statistics.AddRange(new[]
            {
                state.GlobalStats,
                state.Pool.Statistics,
                state.Queue.Statistics
            });
            return state;
        }


        public override ExitResult SolveInner(SolverStateEvaluationSingleThreaded state)
        {
            base.SolveInner(state);
            
            if (state.Exit == ExitResult.QueueEmpty) state.Exit = ExitResult.ExhaustedTree;
            return state.Exit;
        }
        


    }
}