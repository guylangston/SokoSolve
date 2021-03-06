﻿using System.Linq;
using SokoSolve.Core.Solver.Lookup;
using SokoSolve.Core.Solver.Queue;

namespace SokoSolve.Core.Solver.Solvers
{
    public sealed class SingleThreadedForwardSolver : SolverBase<SolverStateEvaluationSingleThreaded>
    {
        private readonly ISolverNodePoolingFactory nodePoolingFactory;

        public SingleThreadedForwardSolver(ISolverNodePoolingFactory nodePoolingFactory) 
            : base(2, 0, "Single Threaded Forward-only Solver")
        {
            this.nodePoolingFactory = nodePoolingFactory;
        }

        public override ExitResult Solve(SolverState state)
        {
            var ss = (SolverStateEvaluationSingleThreaded)state;
            return SolveInner(ss, (TreeState)ss.TreeState);
        }


        public override SolverStateEvaluationSingleThreaded Init(SolverCommand command)
        {
            var eval  = new ForwardEvaluator(command, nodePoolingFactory);
            var pool  = command.ServiceProvider.GetInstanceElseDefault<INodeLookup>(() => new NodeLookupSimpleList());
            var queue = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(() => new SolverQueue());
            var root  = eval.Init(command, queue, pool);
            
            pool.Add(root.Recurse().ToArray());

            var state = InitState(command, 
                new SolverStateEvaluationSingleThreaded(command, this, new TreeState(root, pool, queue, eval)));
            
            state.Statistics.AddRange(new[]
            {
                state.GlobalStats,
                state.TreeState.Pool.Statistics,
                state.TreeState.Queue.Statistics
            });
            return state;
        }


        protected override ExitResult SolveInner(SolverStateEvaluationSingleThreaded state, TreeState tree)
        {
            base.SolveInner(state, tree);
            
            if (state.Exit == ExitResult.QueueEmpty) state.Exit = ExitResult.ExhaustedTree;
            return state.Exit;
        }
        


    }
}