using System.Linq;
using SokoSolve.Core.Solver.Lookup;
using SokoSolve.Core.Solver.Queue;

namespace SokoSolve.Core.Solver.Solvers
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
            var eval      = new ReverseEvaluator(command, nodePoolingFactory);
            var pool      = command.ServiceProvider.GetInstanceElseDefault<INodeLookup>(() => new NodeLookupSimpleList());
            var queue     = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(() => new SolverQueue());
            
            var root = eval.Init(command, queue, pool);
            
            var treeState = new TreeState(root, pool, queue, eval);
            var state = InitState(command, new SolverStateEvaluationSingleThreaded(command, this, treeState));
            
            
            
            pool.Add(root.Recurse().ToArray());

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