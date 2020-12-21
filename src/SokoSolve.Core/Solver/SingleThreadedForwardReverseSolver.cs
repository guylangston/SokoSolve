using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Analytics;

namespace SokoSolve.Core.Solver
{
    /// <summary>
    ///     This is more useful for testing
    /// </summary>
    public class SingleThreadedForwardReverseSolver : ISolver
    {
        public virtual int VersionMajor => 2;
        public virtual int VersionMinor => 1;
        public int VersionUniversal => SolverHelper.VersionUniversal;
        public virtual string VersionDescription =>
            "Single-threaded logic for solving a Reverse and a Forward solver on a SINGLE pool";

        private readonly ISolverNodePoolingFactory nodePoolingFactory;
        public SingleThreadedForwardReverseSolver(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory)
        {
            this.nodePoolingFactory = nodePoolingFactory;
        }
        
        public string                                  TypeDescriptor                                 => null;
        public IEnumerable<(string name, string? text)> GetTypeDescriptorProps(SolverState state) => null;

        public virtual SolverState Init(SolverCommand command)
        {
            var state = new SingleThreadedSolverState(command, this)
            {
                Forward = new SolverTreeState
                {
                    Evaluator   = new ForwardEvaluator(command, nodePoolingFactory),
                    Queue       = new SolverQueue(),
                    PoolForward = new NodeLookupSimpleList()
                },
                Reverse = new SolverTreeState
                {
                    Evaluator   = new ReverseEvaluator(command, nodePoolingFactory),
                    Queue       = new SolverQueue(),
                    PoolReverse = new NodeLookupSimpleList()
                }
            };
            state.Forward.PoolReverse = state.Reverse.PoolReverse;
            state.Reverse.PoolForward = state.Forward.PoolForward;

            state.StaticMaps = new StaticAnalysisMaps(command.Puzzle);
            
            state.Forward.Root = state.Forward.Evaluator.Init(command.Puzzle, state.Forward.Queue);
            state.Forward.PoolForward.Add(state.Forward.Root.Recurse().ToList());
            
            state.Reverse.Root = state.Reverse.Evaluator.Init(command.Puzzle, state.Reverse.Queue);
            state.Reverse.PoolReverse.Add(state.Reverse.Root.Recurse().ToList());
            state.Statistics.AddRange(new[]
            {
                state.GlobalStats,
                state.Forward.PoolForward.Statistics,
                state.Reverse.PoolReverse.Statistics,
                state.Forward.Queue.Statistics,
                state.Reverse.Queue.Statistics
            });

            return state;
        }

        public IEnumerable<(string name, string? text)> GetSolverDescriptionProps(SolverState state)
        {
            if (state is SingleThreadedSolverState res)
            {
                yield return ("Pool.Forward",  res.Forward?.PoolForward?.GetType().Name);
                yield return ("Queue.Forward", res.Forward?.Queue?.GetType().Name);
                yield return ("Pool.Reverse",  res.Reverse?.PoolReverse?.GetType().Name);
                yield return ("Queue.Reverse", res.Reverse?.Queue?.GetType().Name);
            }
            else
            {
                throw new Exception();
            }
            
        }

        public ExitResult Solve(SolverState state) => Solver((SingleThreadedSolverState) state);

        public ExitResult Solver(SingleThreadedSolverState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Forward == null) throw new ArgumentNullException(nameof(state.Forward));
            if (state.Forward.PoolForward == null) throw new ArgumentNullException(nameof(state.Forward));
            if (state.Forward.PoolReverse == null) throw new ArgumentNullException(nameof(state.Forward));
            if (state.Reverse == null) throw new ArgumentNullException(nameof(state.Reverse));
            if (state.Reverse.PoolForward == null) throw new ArgumentNullException(nameof(state.Reverse));
            if (state.Reverse.PoolReverse == null) throw new ArgumentNullException(nameof(state.Reverse));
            
            const int tick = 1000;
            
            state.GlobalStats.TotalNodes = 0;
            while (true)
            {
                if (!DequeueAndEval(state, state.Forward, state.Forward.PoolForward, state.Forward.PoolReverse))
                {
                    return state.Exit;
                }
                
                if (!DequeueAndEval(state, state.Reverse, state.Reverse.PoolReverse, state.Reverse.PoolForward))
                {
                    return state.Exit;
                }

                if (state.GlobalStats.TotalNodes % tick == 0 && CheckExit(state))
                {
                    return state.Exit;
                }
            }
        }

        private bool CheckExit(SingleThreadedSolverState state)
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

        private bool DequeueAndEval(SingleThreadedSolverState state, SolverTreeState part, INodeLookup pool,
            INodeLookup solution)
        {
            var node = part.Queue.Dequeue();
            if (node == null)
            {
                state.Exit = ExitResult.ExhaustedTree;
                return false;
            };

            if (part.Evaluator.Evaluate(state, part.Queue, pool, solution, node))
            {
                // Solution
                if (state.Command.ExitConditions.StopOnSolution)
                    return false;
            }
                
            state.GlobalStats.TotalNodes++;
            return true;
        }

       

        
    }
}