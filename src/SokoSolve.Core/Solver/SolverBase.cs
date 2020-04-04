using System;
using System.Collections.Generic;
using System.Resources;
using System.Threading;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
   

    public abstract class SolverBase : ISolver
    {
        private readonly INodeEvaluator evaluator;

        protected SolverBase(INodeEvaluator evaluator)
        {
            
            this.evaluator = evaluator;
            BatchSize      = 150;
        }

        public         int                BatchSize          { get; set; }
        public         SolverStatistics[]? Statistics        { get; protected set; }
        public virtual int                VersionMajor       => 1;
        public virtual int                VersionMinor       => 1;
        public         int                VersionUniversal   => SolverHelper.VersionUniversal;
        public virtual string             VersionDescription => "Core logic for solving a path tree";

        public virtual SolverCommandResult Init(SolverCommand command)
        {
            var state = SolverHelper.Init(new CommandResult(), command);

            state.Statistics.Name = GetType().Name;
            state.Pool            = new SolverNodeLookupByBucket();
            state.Evaluator       = evaluator;
            state.Queue           = new SolverQueue();
            state.Root            = state.Evaluator.Init(command.Puzzle, state.Queue);

            Statistics = new[] {state.Statistics, state.Pool.Statistics, state.Queue.Statistics};
            return state;
        }

        public string                                  GetTypeDescriptor                                 => null;
        public virtual IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverCommandResult state) => throw new NotSupportedException();
        
        
        public void Solve(SolverCommandResult state)
        {
            Solve(state as CommandResult);
        }
        
        public virtual void Solve(CommandResult state)
        {
            if (state == null) throw new ArgumentNullException("state");
            if (state.Queue == null) throw new ArgumentNullException("state.Queue");
            if (state.Evaluator == null) throw new ArgumentNullException("state.Evaluator");
            if (state.Statistics == null) throw new ArgumentNullException("state.Statistics");
            
            state.Statistics.Started = DateTime.Now;

            const int tick       = 1000;
            var       sleepCount = 0;
            const int maxSleeps  = 10;
            int       loopCount  = 0;
            while (true)
            {
                var batch = state.Queue.Dequeue(BatchSize);
                if (batch != null && batch.Length > 0)
                {
                    foreach (var next in batch)
                        if (next.Status == SolverNodeStatus.UnEval)
                        {
                            // Evaluate
                            if (state.Evaluator.Evaluate(state, state.Queue, state.Pool, null, next))
                            {
                                // Solution
                                if (state.Command.ExitConditions.StopOnSolution)
                                {
                                    state.Pool.Add(next);
                                    state.Statistics.Completed = DateTime.Now;
                                    state.Exit                 = ExitConditions.Conditions.Solution;
                                    
                                    SolverHelper.GetSolutions(state, true);
                                    return;
                                }
                            }

                            // Manage Statistics
                            state.Statistics.TotalNodes++;
                            var d = next.GetDepth();
                            if (d > state.Statistics.DepthMax) state.Statistics.DepthMax = d;
                            state.Statistics.DepthCurrent = d;

                            // Every x-nodes check the control/exit conditions
                            if (loopCount++ % tick == 0)
                            {
                                state.PeekOnTick = next;
                                if (Tick(state.Command, state, state.Queue, out var solve))
                                {
                                    state.Exit = solve.Exit;
                                    SolverHelper.GetSolutions(state, true);
                                    return;
                                }
                            }
                        }
                }
                else
                {
                    Thread.Sleep(100);
                    if (sleepCount++ == maxSleeps)
                    {
                        state.Exit = ExitConditions.Conditions.NothingLeftToDo;
                        SolverHelper.GetSolutions(state, true);
                        return;
                    }
                }
            }
        }

        protected virtual bool Tick(
            SolverCommand command, 
            CommandResult state, 
            ISolverQueue queue,
            out SolverCommandResult solve)
        {
            state.Statistics.DepthCompleted = queue.Statistics.DepthCompleted;
            state.Statistics.DepthMax       = queue.Statistics.DepthMax;

            if (command.Progress != null) command.Progress.Update(this, state, state.Statistics);

            if (command.CheckAbort != null)
                if (command.CheckAbort(command))
                {
                    state.Exit                 = ExitConditions.Conditions.Aborted;
                    state.EarlyExit            = true;
                    state.Statistics.Completed = DateTime.Now;
                    solve                      = state;
                    return true;
                }

            var check = command.ExitConditions.ShouldExit(state);
            if (check != ExitConditions.Conditions.Continue)
            {
                state.EarlyExit            = true;
                state.Statistics.Completed = DateTime.Now;
                state.Exit                 = check;
                solve                      = state;
                return true;
            }

            solve = null;
            return false;
        }

        public class CommandResult : SolverCommandResult, ISolverVisualisation
        {
            public SolverNode?        Root      { get; set; }
            public ISolverQueue?      Queue     { get; set; }
            public ISolverNodeLookup? Pool      { get; set; }
            public INodeEvaluator?    Evaluator { get; set; }
            public SolverNode?        PeekOnTick { get; set; }
            
            public bool TrySample(out SolverNode node)
            {
                node = PeekOnTick;
                return PeekOnTick != null;
            }
        }

       
    }
}