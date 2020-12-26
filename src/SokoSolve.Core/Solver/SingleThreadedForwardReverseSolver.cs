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
            var fwdEval  = new ForwardEvaluator(command, nodePoolingFactory);
            var fwdQueue = new SolverQueue();
            var fwdRoot  = fwdEval.Init(command.Puzzle, fwdQueue);
            var fwdPool  = new NodeLookupSimpleList();
            fwdPool.Add(fwdRoot.Recurse().ToList());
            
            
            var revEval  = new ReverseEvaluator(command, nodePoolingFactory);
            var revQueue = new SolverQueue();
            var revRoot  = revEval.Init(command.Puzzle, revQueue);
            var revPool  = new NodeLookupSimpleList();
            revPool.Add(revRoot.Recurse().ToList());
            
            var state = new SolverStateForwardReverse(command, this)
            {
                Forward = new SolverStateEvaluationSingleThreaded(
                    command, this,
                    fwdEval,
                    fwdRoot,
                    fwdPool,
                    fwdQueue,
                    revPool, revQueue),
                Reverse = new SolverStateEvaluationSingleThreaded(
                    command, this,
                    revEval,
                    revRoot,
                    revPool,
                    revQueue,
                    fwdPool, fwdQueue),
            };
            
            state.StaticMaps = new StaticAnalysisMaps(command.Puzzle);
            
            
            state.Statistics.AddRange(new[]
            {
                state.GlobalStats,
                state.Forward.Queue.Statistics,
                state.Reverse.Queue.Statistics
            });

            return state;
        }

        public IEnumerable<(string name, string? text)> GetSolverDescriptionProps(SolverState state)
        {
            if (state is SolverStateForwardReverse res)
            {
                yield return ("Queue.Forward", res.Forward?.Queue?.GetType().Name);
                yield return ("Queue.Reverse", res.Reverse?.Queue?.GetType().Name);
            }
            else
            {
                throw new Exception();
            }
            
        }

        public ExitResult Solve(SolverState state) => Solver((SolverStateForwardReverse) state);

        public ExitResult Solver(SolverStateForwardReverse state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Forward == null) throw new ArgumentNullException(nameof(state.Forward));
            if (state.Reverse == null) throw new ArgumentNullException(nameof(state.Reverse));
            
            const int tick = 1000;
            
            state.GlobalStats.TotalNodes = 0;
            while (true)
            {
                if (!DequeueAndEval(state, state.Forward))
                {
                    return state.Exit;
                }
                
                if (!DequeueAndEval(state, state.Reverse))
                {
                    return state.Exit;
                }

                if (state.GlobalStats.TotalNodes % tick == 0 && CheckExit(state))
                {
                    return state.Exit;
                }
            }
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

        private bool DequeueAndEval(SolverStateForwardReverse owner, SolverStateEvaluation worker )
        {
            var node = worker.Queue.Dequeue();
            if (node == null)
            {
                owner.Exit = ExitResult.ExhaustedTree;
                return false;
            };

            if (worker.Evaluator.Evaluate(worker, node))
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