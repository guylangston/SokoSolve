using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver.Lookup;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public sealed class SingleThreadedReverseSolver : SolverBase<SolverStateEvaluationSingleThreaded>
    {
        private readonly ISolverNodePoolingFactory nodePoolingFactory;
        public SingleThreadedReverseSolver(ISolverNodePoolingFactory nodePoolingFactory) 
            : base(2, 0, "Single Threaded Reverse-only Solver")
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
            var eval  = new ReverseEvaluator(command, nodePoolingFactory);
            var queue = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(() => new SolverQueue());
            var root  = eval.Init(command.Puzzle, queue);
            var pool  = command.ServiceProvider.GetInstanceElseDefault<INodeLookup>(() => new NodeLookupSimpleList());
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