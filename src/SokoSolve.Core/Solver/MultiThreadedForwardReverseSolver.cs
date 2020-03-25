using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SokoSolve.Core.Solver
{
    public class MultiThreadedForwardReverseSolver : ISolver
    {
        private CommandResult? current = null;

        public MultiThreadedForwardReverseSolver()
        {
            var total = Environment.ProcessorCount;
            ThreadCountForward = ThreadCountReverse = total / 2;
        }

        public virtual int VersionMajor       => 2;
        public virtual int VersionMinor       => 2;
        public         int VersionUniversal   => SolverHelper.VersionUniversal;
        public         int ThreadCountForward { get; set; }
        public         int ThreadCountReverse { get; set; }

        public virtual string VersionDescription =>
            "Multi-threaded logic for solving a set of Reverse and a set of Forward streams on a SINGLE pool";

        public SolverStatistics[] Statistics => current?.StatsInner.ToArray();

        public SolverCommandResult Init(SolverCommand command)
        {
            var prog = command.Progress;
            command.Progress = null;

            var poolForward = command.ServiceProvider != null
                ? command.ServiceProvider.GetService<ISolverNodeLookup>()
                : new SolverNodeLookupThreadSafeBuffer();
            
            var poolReverse = command.ServiceProvider != null
                ? command.ServiceProvider.GetService<ISolverNodeLookup>()
                : new SolverNodeLookupThreadSafeBuffer();
            
            var queueForward = command.ServiceProvider != null
                ? command.ServiceProvider.GetService<ISolverQueue>()
                :  new SolverQueueConcurrent();
            
            var queueReverse = command.ServiceProvider != null
                ? command.ServiceProvider.GetService<ISolverQueue>()
                : new SolverQueueConcurrent();
            
            poolForward.Statistics.Name  = "Forward Pool";
            poolReverse.Statistics.Name  = "Reverse Pool";
            queueForward.Statistics.Name = "Forward Queue";
            queueReverse.Statistics.Name = "Reverse Queue";
            
            current = new CommandResult
            {
                PoolForward = poolForward,
                PoolReverse = poolReverse,
                Command     = command,
                Statistics = new SolverStatistics
                {
                    Name    = GetType().Name,
                    Started = DateTime.Now
                },
                StatsInner = new List<SolverStatistics>(),
                Workers    = new List<Worker>()
            };

            for (int i = 0; i < ThreadCountForward; i++)
            {
                current.Workers.Add(new ForwardWorker
                {
                    Name         = "F" + i.ToString(),
                    Command      = command,
                    Pool         = poolForward,
                    PoolSolution = poolReverse,
                    Queue        = queueForward
                });
            }
            for (int i = 0; i < ThreadCountReverse; i++)
            {
                current.Workers.Add(new ReverseWorker
                {
                    Name         = "R"+i.ToString(),
                    Command      = command,
                    Pool         = poolReverse,
                    PoolSolution = poolForward,
                    Queue        = queueReverse
                });
            }

            current.StatsInner.Add(current.Statistics);
            current.StatsInner.Add(poolForward.Statistics);
            
            current.StatsInner.Add(poolReverse.Statistics);
            
            current.StatsInner.Add(queueForward.Statistics);
            current.StatsInner.Add(queueReverse.Statistics);

            var tmp = command.Progress;
            command.Progress = null;
            foreach (var worker in current.Workers)
            {
                worker.Owner = this;
                worker.OwnerState = current;
                worker.Init();
                if (worker.Solver.Statistics != null) current.StatsInner.AddRange(worker.Solver.Statistics);
                worker.Task = new Task<Worker>(x => Execute((Worker) x), worker, TaskCreationOptions.LongRunning);
            }

            command.Progress = tmp;

            // Init queues
            current.Workers.First(X => X.Evaluator is ForwardEvaluator).Evaluator.Init(command.Puzzle, queueForward);
            current.Workers.First(X => X.Evaluator is ReverseEvaluator).Evaluator.Init(command.Puzzle, queueReverse);

            command.Progress = prog;

            return current;
        }

        public void Solve(SolverCommandResult state)
        {
            var full     = (CommandResult) state;
            var allTasks = full.Workers.Select(x => (Task) x.Task).ToArray();
            var cancel   = state.Command.CancellationToken;
            
            // Start and wait
            full.IsRunning = true;
            foreach (var worker in full.Workers) worker.Task.Start();
            Task.WaitAll(allTasks, (int) state.Command.ExitConditions.Duration.TotalMilliseconds, cancel);
            full.IsRunning = false;
            state.Statistics.Completed = DateTime.Now;

            foreach (var stat in current.StatsInner)
            {
                stat.Completed = state.Statistics.Completed;
            }

            // Get solutions
            foreach (var worker in full.Workers)
            {
                worker.WorkerCommandResult.Statistics.Completed = state.Statistics.Completed;
                
                if (worker.WorkerCommandResult.HasSolution)
                {
                    if (worker.WorkerCommandResult.SolutionsNodes != null)
                    {
                        full.SolutionsNodes ??= new List<SolverNode>();
                        full.SolutionsNodes.AddRange(worker.WorkerCommandResult.SolutionsNodes);
                        state.Exit = ExitConditions.Conditions.Solution;
                    }

                    if (worker.WorkerCommandResult.SolutionsNodesReverse != null)
                    {
                        full.SolutionsNodesReverse ??= new List<SolutionChain>();
                        full.SolutionsNodesReverse.AddRange(worker.WorkerCommandResult.SolutionsNodesReverse);
                        state.Exit = ExitConditions.Conditions.Solution;
                    }
                }
            }
            // Update stats
            state.Statistics.TotalNodes = current.PoolForward.Statistics.TotalNodes 
                                          + current.PoolReverse.Statistics.TotalNodes;

            SolverHelper.GetSolutions(state, true);
            
            // Check for errors
            var errors = full.Workers.Select(x => x.WorkerCommandResult.Exception).Where(x => x != null).ToList();
            if (errors.Any())
            {
                throw new AggregateException(errors);
            }
            
            if (state.Exit == ExitConditions.Conditions.Continue)
            {
                state.Exit = full.Workers.Select(x => x.WorkerCommandResult.Exit)
                            .GroupBy(x => x)
                            .OrderBy(x => x.Count())
                            .First().Key;
            }

           
            
            // TODO: Close off other Inner Stats
        }

       

        private Worker Execute(Worker worker)
        {
            try
            {
                worker.Solve();
                if (worker.WorkerCommandResult.HasSolution && worker.OwnerState.Command.ExitConditions.StopOnSolution)
                    worker.OwnerState.IsRunning = false;
            }
            catch (Exception ex)
            {
                worker.WorkerCommandResult.Exception = ex;
            }

            return worker;
        }

        public abstract class Worker
        {
            public          INodeEvaluator                    Evaluator           { get; set; }
            public          ISolverQueue                      Queue               { get; set; }
            public          ISolverNodeLookup                 Pool                { get; set; }
            public          ISolverNodeLookup                 PoolSolution        { get; set; }
            public          string                            Name                { get; set; }
            public          SolverCommand                     Command             { get; set; }
            public          SolverCommandResult               WorkerCommandResult { get; set; }
            public          ISolver                           Solver              { get; set; }
            public          Task<Worker>                      Task                { get; set; }
            public          MultiThreadedForwardReverseSolver Owner               { get; set; }
            public          CommandResult                     OwnerState          { get; set; }
            public          Thread                            Thread              { get; set; }
            public abstract void                              Init();

            public virtual void Solve()
            {
                Solver.Solve(WorkerCommandResult);
            }
        }


        private class ForwardWorker : Worker
        {
            public override void Init()
            {
                Evaluator = new ForwardEvaluator();
                Solver = new ForwardSolver
                {
                    Worker = this
                };
                WorkerCommandResult = Solver.Init(Command);
            }
        }

        private class ReverseWorker : Worker
        {
            public override void Init()
            {
                Evaluator = new ReverseEvaluator();
                Solver = new ReverseSolver
                {
                    Worker = this
                };
                WorkerCommandResult = Solver.Init(Command);
            }
        }

        private class ForwardSolver : SingleThreadedForwardSolver
        {
            public Worker Worker { get; set; }

            public override SolverCommandResult Init(SolverCommand command)
            {
                var state = SolverHelper.Init(new CommandResult(), command);
                state.Command.Parent     = Worker.Owner;
                state.Command.CheckAbort = x => !Worker.OwnerState.IsRunning;
                state.Statistics.Name    = GetType().Name;
                state.Pool               = Worker.Pool;
                state.Evaluator          = new ForwardEvaluator();
                state.Queue              = Worker.Queue;

                Statistics = new[] {state.Statistics};
                return state;
            }
        }

        private class ReverseSolver : SingleThreadedReverseSolver
        {
            public Worker Worker { get; set; }

            public override SolverCommandResult Init(SolverCommand command)
            {
                var state = SolverHelper.Init(new CommandResult(), command);
                state.Command.Progress   = null;
                state.Command.Parent     = Worker.Owner;
                state.Command.CheckAbort = CheckWorkerAbort;
                state.Statistics.Name    = GetType().Name;
                state.Pool               = Worker.Pool;
                state.Evaluator          = new ReverseEvaluator();
                state.Queue              = Worker.Queue;

                Statistics = new[] {state.Statistics};
                return state;
            }

            private bool CheckWorkerAbort(SolverCommand arg)
            {
                return !Worker.OwnerState.IsRunning;
            }
        }


        public class CommandResult : SolverCommandResult, ISolverVisualisation
        {
            public List<Worker> Workers { get; set; }
            public List<SolverStatistics> StatsInner { get; set; }
            public bool IsRunning { get; set; }
            public ISolverNodeLookup PoolReverse { get; set; }
            public ISolverNodeLookup PoolForward { get; set; }
            
            public bool TrySample(out SolverNode node)
            {
                return PoolForward.TrySample(out node);
            }
        }

       
    }
}