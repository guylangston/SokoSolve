using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SokoSolve.Core.Solver.Lookup;

namespace SokoSolve.Core.Solver
{
    public abstract class SolverBase : ISolver
    {
        private readonly INodeEvaluator evaluator;

        protected SolverBase(INodeEvaluator evaluator)
        {
            this.evaluator = evaluator;
            BatchSize      = 10;
        }

        public         int    BatchSize          { get; set; }
        public virtual int    VersionMajor       => 1;
        public virtual int    VersionMinor       => 1;
        public         int    VersionUniversal   => SolverHelper.VersionUniversal;
        public virtual string VersionDescription => "Core logic for solving a path tree";

        
        public virtual SolverState Init(SolverCommand command)
        {
            var state = SolverHelper.Init(new SolverBaseState(command, this), command);

            state.GlobalStats.Name = GetType().Name;
            state.Pool             = command.ServiceProvider.GetInstanceElseDefault<INodeLookup>(()=> new NodeLookupSimpleList());
            state.Queue            = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(()=>new SolverQueue());
            state.Evaluator        = evaluator;
            state.Root             = state.Evaluator.Init(command.Puzzle, state.Queue);
            state.Pool.Add(state.Root.Recurse().ToList());

            state.Statistics.AddRange(new[]
            {
                state.GlobalStats,
                state.Pool.Statistics,
                state.Queue.Statistics
            });
            return state;
        }

        public string TypeDescriptor => GetType().Name;
        public virtual IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => null;
        
        public ExitResult Solve(SolverState state) => Solve((SolverBaseState)state);

        public virtual ExitResult Solve(SolverBaseState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Queue == null) throw new ArgumentNullException(nameof(state.Queue));
            if (state.Evaluator == null) throw new ArgumentNullException(nameof(state.Evaluator));
            if (state.GlobalStats == null) throw new ArgumentNullException(nameof(state.GlobalStats));
            if (state.Command == null) throw new ArgumentNullException(nameof(state.Command));
            
            state.GlobalStats.Started = DateTime.Now;

            const int tick       = 1000;
            var       sleepCount = 0;
            const int maxSleeps  = 10;
            int       loopCount  = 0;
            while (true)
            {
                if (state.Command.CheckExit(state, out var exit))
                {
                    return exit;
                }
                
                var batch = state.Queue.Dequeue(BatchSize);
                if (batch != null && batch.Count > 0)
                {
                    sleepCount = 0;
                    foreach (var next in batch)
                    {
                        if (state.Command.CheckExit(state, out var exitInner))
                        {
                            return exitInner;
                        }

                        if (Check(state, next)) // Check if still valid for eval
                        {
                            // Evaluate
                            INodeLookup? solutionPoolAlt = null;
                            if (state is MultiThreadedSolverState multi && multi.PoolReverse is not null)
                            {
                                solutionPoolAlt = multi.PoolReverse;
                            }
                            if (state.Evaluator.Evaluate(state, state.Queue, state.Pool, solutionPoolAlt, next))
                            {
                                // Solution
                                if (state.Command.ExitConditions.StopOnSolution)
                                {
                                    state.GlobalStats.Completed = DateTime.Now;
                                    state.Exit                 = ExitResult.Solution;
                                    return state.Exit;
                                }
                            }

                            // Manage Statistics
                            var d = next.GetDepth();
                            if (d > state.GlobalStats.DepthMax) state.GlobalStats.DepthMax = d;
                            state.GlobalStats.DepthCurrent = d;

                            // Every x-nodes check the control/exit conditions
                            if (loopCount++ % tick == 0)
                            {
                                state.PeekOnTick = next;
                                if (Tick(state.Command, state, state.Queue, out var solve))
                                {
                                    state.Exit = solve.Exit;
                                    return state.Exit;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100);
                    if (sleepCount++ == maxSleeps)
                    {
                        state.Exit = ExitResult.QueueEmpty;
                        return state.Exit;
                    }
                }
            }
        }
        
        protected  virtual bool Check(SolverBaseState solverBaseState, SolverNode next)
        {
            if (next.Status != SolverNodeStatus.UnEval) return false;
            
            // Still valid? No needed for multi-threading
            // May have been added/evaluated by another node already
            var match = solverBaseState.Pool.FindMatch(next);
            if (match != null && match.Status != SolverNodeStatus.UnEval)
            {
                return false;
            }

            return true;
        }

        protected virtual bool Tick(
            SolverCommand command, 
            SolverBaseState state, 
            ISolverQueue queue,
            out SolverState solve)
        {
            state.GlobalStats.DepthCompleted = queue.Statistics.DepthCompleted;
            state.GlobalStats.DepthMax       = queue.Statistics.DepthMax;

            if (command.Progress != null) command.Progress.Update(this, state, state.GlobalStats, state.GlobalStats.ToString());

            if (state.Command.CheckExit(state, out var exit))
            {
                state.Exit                 = exit;
                state.EarlyExit = exit switch
                {
                    ExitResult.Aborted => true,
                    _ => false
                };
                state.GlobalStats.Completed = DateTime.Now;
                solve                      = state;
                return true;
            }
            

            solve = null;
            return false;
        }

       

       
    }
}