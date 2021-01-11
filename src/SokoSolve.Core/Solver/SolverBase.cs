using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Solver.Components;

namespace SokoSolve.Core.Solver
{
    public abstract class SolverBase<TState> : ISolver where TState: SolverState
    {
        protected SolverBase(int versionMajor, int versionMinor, string versionDescription)
        {
            BatchSize          = 1;
            VersionMajor       = versionMajor;
            VersionMinor       = versionMinor;
            VersionDescription = versionDescription;
        }

        public         int    BatchSize          { get; }  // Reduce Locking pressure
        public         int    VersionUniversal   => SolverHelper.VersionUniversal;
        public virtual int    VersionMajor       { get; }
        public virtual int    VersionMinor       { get; }
        public virtual string VersionDescription { get; }

        SolverState ISolver.Init(SolverCommand command) => this.Init(command);
        public abstract ExitResult Solve(SolverState state);

        
        public static TState InitState(SolverCommand command, TState res)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (command.ExitConditions == null) throw new NullReferenceException(nameof(command.ExitConditions));
            if (command.Puzzle == null) throw new NullReferenceException(nameof(command.Puzzle));
            
            if (!command.Puzzle.IsValid(out string err))
            {
                throw new InvalidDataException($"Not a valid puzzle: {err}");
            }

            res.GlobalStats.Started = DateTime.Now;
            res.StaticMaps          = new StaticAnalysisMaps(command.Puzzle);
            
            // Optional Components
            if (command.ServiceProvider != null)
            {
                res.KnownSolutionTracker = command.ServiceProvider.GetInstance<IKnownSolutionTracker>();
                res.KnownSolutionTracker?.Init(res);

            }
            
            return res;
        }
        
        public abstract TState Init(SolverCommand command);

        protected virtual ExitResult SolveInner(TState state, TreeState tree)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (tree == null) throw new ArgumentNullException(nameof(tree));
            if (state.Command == null) throw new ArgumentNullException(nameof(state.Command));
            if (state.GlobalStats == null) throw new ArgumentNullException(nameof(state.GlobalStats));

            state.GlobalStats.Started = DateTime.Now;

            List<SolverNode> fromQueue = new List<SolverNode>();

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
                
                fromQueue.Clear();
                if (tree.Queue.Dequeue(BatchSize, fromQueue))
                {
                    sleepCount = 0;
                    foreach (var next in fromQueue)
                    {
                        if (state.Command.CheckExit(state, out var exitInner))
                        {
                            return exitInner;
                        }

                        if (Check(state, tree, next)) // Check if still valid for eval
                        {
                            // Evaluate
                            if (tree.Evaluator.Evaluate(state, tree, next))
                            {
                                // Solution
                                if (state.Command.ExitConditions.StopOnSolution)
                                {
                                    state.GlobalStats.Completed = DateTime.Now;
                                    state.Exit                 = ExitResult.Solution;
                                    return state.Exit;
                                }
                            }
                            
                            // Tracking
                            state.KnownSolutionTracker?.EvalComplete(state, tree, next);

                            // Manage Statistics
                            var d = next.GetDepth();
                            if (d > state.GlobalStats.DepthMax) state.GlobalStats.DepthMax = d;
                            state.GlobalStats.DepthCurrent = d;

                            // Every x-nodes check the control/exit conditions
                            if (loopCount++ % tick == 0)
                            {
                                if (Tick(state.Command, state, tree, out var solve))
                                {
                                    state.Exit = solve.Exit;
                                    return state.Exit;
                                }
                            }
                        }
                        else
                        {
                            // Will never be added to the Pool (some other thread has already found this node)
                            next.Status = SolverNodeStatus.Duplicate;
                            state.GlobalStats.Duplicates++;
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
        
        protected bool Check(TState state, TreeState treeState, SolverNode next)
        {
            if (state is SolverStateMultiThreaded.WorkerState ms)
            {
                if (next.Status != SolverNodeStatus.UnEval) return false;
                
                // Already processed?
                if (state.Command.ConfirmDequeNotAlreadyProcessed && treeState.Pool.FindMatch(next) != null)
                {
                    state.GlobalStats.Warnings++;
                    return false;
                }
            }

            return true;
        }

        protected virtual bool Tick(
            SolverCommand command, 
            TState state, 
            TreeState tree,
            out SolverState solve)
        {
            state.GlobalStats.DepthCompleted = tree.Queue.Statistics.DepthCompleted;
            state.GlobalStats.DepthMax       = tree.Queue.Statistics.DepthMax;

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

        
        public string TypeDescriptor => GetType().Name;
        public virtual IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => null;
       

       
    }
}