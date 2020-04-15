using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SokoSolve.Core.Solver
{
    
    public class MultiThreadedSolverBaseResult : SolverBaseResult
    {
        public List<MultiThreadedForwardReverseSolver.Worker>?           Workers      { get; set; }
        public List<SolverStatistics>? StatsInner   { get; set; }
        public bool                    IsRunning    { get; set; }
        public ISolverPool?            PoolReverse  { get; set; }
        public ISolverPool?            PoolForward  { get; set; }
        public ISolverQueue            QueueForward { get; set; }
        public ISolverQueue            QueueReverse { get; set; }
    }
    public class MultiThreadedForwardReverseSolver : ISolver
    {
        private readonly ISolverNodeFactory nodeFactory;
        private MultiThreadedSolverBaseResult? current = null;

        public MultiThreadedForwardReverseSolver(ISolverNodeFactory nodeFactory)
        {
            this.nodeFactory = nodeFactory;
            var total = Environment.ProcessorCount;
            ThreadCountForward = ThreadCountReverse = (total / 2);
        }

        public int ThreadCountForward { get; set; }
        public int ThreadCountReverse { get; set; }
        public int VersionMajor       => 2;
        public int VersionMinor       => 2;
        public int VersionUniversal   => SolverHelper.VersionUniversal;
        public string VersionDescription =>
            "Multi-threaded logic for solving a set of Reverse and a set of Forward streams on a SINGLE pool";

        public SolverStatistics[] Statistics => current?.StatsInner.ToArray();
        
        public string TypeDescriptor => $"{GetType().Name}:fr! ==> {nodeFactory}";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state)
        {
            yield return ("Strategy.ShortName", "fr!");
            if (state is MultiThreadedSolverBaseResult cc)
            {
                yield return ("Pool.Forward", cc.PoolForward?.TypeDescriptor);
                yield return ("Pool.Reverse", cc.PoolReverse?.TypeDescriptor);
                yield return ("Queue.Forward", cc.QueueForward?.TypeDescriptor);
                yield return ("Queue.Reverse", cc.QueueReverse?.TypeDescriptor);
            }
            else
            {
                throw new InvalidDataException(state?.GetType().Name);
            }

        }


        public SolverResult Init(SolverCommand command)
        {

            var poolForward = command.ServiceProvider.GetInstanceElseDefault<ISolverPool>(() => new SolverPoolSlimLockWithLongTerm());
            var poolReverse = command.ServiceProvider.GetInstanceElseDefault<ISolverPool>(() => new SolverPoolSlimLockWithLongTerm());
            
            var queueForward = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(() => new SolverQueueConcurrent());
            var queueReverse = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(() => new SolverQueueConcurrent());
            
            poolForward.Statistics.Name  = "Pool (Forward)";
            poolReverse.Statistics.Name  = "Pool (Reverse)";
            queueForward.Statistics.Name = "Queue (Forward)";
            queueReverse.Statistics.Name = "Queue (Reverse)";
            
            current = new MultiThreadedSolverBaseResult
            {
                PoolForward = poolForward,
                PoolReverse = poolReverse,
                QueueForward = queueForward,
                QueueReverse = queueReverse,
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
                current.Workers.Add(new ForwardWorker(nodeFactory)
                {
                    Name         = $"F{i,00}",
                    Command      = command,
                    Pool         = poolForward,
                    PoolSolution = poolReverse,
                    Queue        = queueForward
                });
            }
            for (int i = 0; i < ThreadCountReverse; i++)
            {
                current.Workers.Add(new ReverseWorker(nodeFactory)
                {
                    Name         = $"R{i,00}",
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

            foreach (var worker in current.Workers)
            {
                worker.Owner = this;
                worker.OwnerState = current;
                worker.Init();

                if (worker.Solver.Statistics != null) current.StatsInner.AddRange(worker.Solver.Statistics);
                worker.Task = new Task<Worker>(
                    x => Execute((Worker) x), 
                    worker, 
                    TaskCreationOptions.LongRunning);
            }



            // Init queues
            current.Workers.First(x => x.Evaluator is ForwardEvaluator).Evaluator.Init(command.Puzzle, queueForward);
            current.Workers.First(x => x.Evaluator is ReverseEvaluator).Evaluator.Init(command.Puzzle, queueReverse);

            
            return current;
        }

     
        public void Solve(SolverResult state)
        {
            var full     = (MultiThreadedSolverBaseResult) state;
            var allTasks = full.Workers.Select(x => (Task) x.Task).ToArray();
            var cancel   = state.Command.CancellationToken;
            
            // Start and wait
            full.IsRunning = true;
            foreach (var worker in full.Workers) worker.Task.Start();
            
            Task statisticsTick = null;
            if (state.Command.AggProgress != null)
            {
                // Setup global/aggregate statistics and updates
                statisticsTick = Task.Run(() =>
                {
                    while (full.IsRunning)
                    {
                        Thread.Sleep(1000);
                        state.Statistics.TotalNodes = current.PoolForward.Statistics.TotalNodes 
                                                      + current.PoolReverse.Statistics.TotalNodes;
                        
                        var txt = $"==> {state.Statistics} F:{current.PoolForward.Statistics.TotalNodes}:{current.QueueForward.Statistics.TotalNodes}, R:{current.PoolReverse.Statistics.TotalNodes}:{current.QueueReverse.Statistics.TotalNodes}";
                        state.Command.AggProgress.Update(this, state, state.Statistics, txt);
                        
                    }
                    if (state.Command.AggProgress is IDisposable dp) dp.Dispose();
                    
                });    
            }

            if (!Task.WaitAll(allTasks, (int) state.Command.ExitConditions.Duration.TotalMilliseconds, cancel))
            {
                state.Exit = ExitConditions.Conditions.TimeOut;
            }
            full.IsRunning = false;
            statisticsTick?.Wait();
            state.Statistics.Completed = DateTime.Now;

            foreach (var stat in current.StatsInner)
            {
                stat.Completed = state.Statistics.Completed;
            }

            // Get solutions
            foreach (var worker in full.Workers)
            {
                worker.WorkerResult.Statistics.Completed = state.Statistics.Completed;
                
                if (worker.WorkerResult.HasSolution)
                {
                    if (worker.WorkerResult.SolutionsNodes != null)
                    {
                        full.SolutionsNodes ??= new List<SolverNode>();
                        full.SolutionsNodes.AddRange(worker.WorkerResult.SolutionsNodes);
                        state.Exit = ExitConditions.Conditions.Solution;
                    }

                    if (worker.WorkerResult.SolutionsNodesReverse != null)
                    {
                        full.SolutionsNodesReverse ??= new List<SolutionChain>();
                        full.SolutionsNodesReverse.AddRange(worker.WorkerResult.SolutionsNodesReverse);
                        state.Exit = ExitConditions.Conditions.Solution;
                    }
                }
            }
            // Update stats
            state.Statistics.TotalNodes = current.PoolForward.Statistics.TotalNodes 
                                          + current.PoolReverse.Statistics.TotalNodes;

            SolverHelper.GetSolutions(state, true);
            
            // Check for errors
            var errors = full.Workers.Select(x => x.WorkerResult.Exception).Where(x => x != null).ToList();
            if (errors.Any())
            {
                throw new AggregateException(errors);
            }
            
            if (state.Exit == ExitConditions.Conditions.Continue)
            {
                state.Exit = full.Workers.Select(x => x.WorkerResult.Exit)
                            .GroupBy(x => x)
                            .OrderBy(x => x.Count())
                            .First().Key;
            }
        }

        private Worker Execute(Worker worker)
        {
            var threadid = Thread.CurrentThread.Name;
            try
            {
                Thread.CurrentThread.Name = worker.Name;
            
                worker.Solve();
                if (worker.WorkerResult.HasSolution && worker.OwnerState.Command.ExitConditions.StopOnSolution)
                    worker.OwnerState.IsRunning = false;
            }
            catch (Exception ex)
            {
                worker.WorkerResult.Exception = ex;
            }
            finally
            {
                //Thread.CurrentThread.Name = threadid;
            }

            return worker;
        }

        public abstract class Worker
        {
            public          INodeEvaluator                    Evaluator           { get; set; }
            public          ISolverQueue                      Queue               { get; set; }
            public          ISolverPool                 Pool                { get; set; }
            public          ISolverPool                 PoolSolution        { get; set; }
            public          string                            Name                { get; set; }
            public          SolverCommand                     Command             { get; set; }
            public          SolverResult               WorkerResult { get; set; }
            public          ISolver                           Solver              { get; set; }
            public          Task<Worker>                      Task                { get; set; }
            public          MultiThreadedForwardReverseSolver Owner               { get; set; }
            public          MultiThreadedSolverBaseResult                     OwnerState          { get; set; }
            public          Thread                            Thread              { get; set; }
            public abstract void                              Init();

            public virtual void Solve()
            {
                Solver.Solve(WorkerResult);
            }
        }


        private class ForwardWorker : Worker
        {
            private readonly ISolverNodeFactory nodeFactory;

            public ForwardWorker(ISolverNodeFactory nodeFactory)
            {
                this.nodeFactory = nodeFactory;
            }

            public override void Init()
            {
                Evaluator = new ForwardEvaluator(nodeFactory);
                Solver = new ForwardSolver(nodeFactory, this);
                WorkerResult = Solver.Init(Command);
                
            }
        }

        private class ReverseWorker : Worker
        {
            private readonly ISolverNodeFactory nodeFactory;

            public ReverseWorker(ISolverNodeFactory nodeFactory)
            {
                this.nodeFactory = nodeFactory;
            }

            public override void Init()
            {
                Evaluator = new ReverseEvaluator(nodeFactory);
                Solver = new ReverseSolver(nodeFactory, this);
                WorkerResult = Solver.Init(Command);
            }
        }

        private class ForwardSolver : SingleThreadedForwardSolver
        {
            private readonly ISolverNodeFactory nodeFactory;

            public ForwardSolver(ISolverNodeFactory nodeFactory, Worker worker) : base(nodeFactory)
            {
                this.nodeFactory = nodeFactory;
                Worker = worker;
            }

            public Worker Worker { get; set; }

            public override SolverResult Init(SolverCommand command)
            {
                var state = SolverHelper.Init(new MultiThreadedSolverBaseResult(), command);
                state.Command.Parent     = Worker.Owner;
                state.Command.CheckAbort = x => !Worker.OwnerState.IsRunning;
                state.Statistics.Name    = $"{GetType().Name}:{Worker.Name}";
                state.Pool               = Worker.Pool;
                state.Evaluator          = new ForwardEvaluator(nodeFactory);
                state.Queue              = Worker.Queue;
                

                Statistics = new[] {state.Statistics};
                return state;
            }
        }

        private class ReverseSolver : SingleThreadedReverseSolver
        {
            private readonly ISolverNodeFactory nodeFactory;

            public ReverseSolver(ISolverNodeFactory nodeFactory, Worker? worker) : base(nodeFactory)
            {
                this.nodeFactory = nodeFactory;
                Worker = worker;
            }

            public Worker? Worker { get; set; }

            public override SolverResult Init(SolverCommand command)
            {
                var state = SolverHelper.Init(new MultiThreadedSolverBaseResult(), command);
                state.Command.Parent     = Worker.Owner;
                state.Command.CheckAbort = CheckWorkerAbort;
                state.Statistics.Name = $"{GetType().Name}:{Worker.Name}";
                state.Pool               = Worker.Pool;
                state.Evaluator          = new ReverseEvaluator(nodeFactory);
                state.Queue              = Worker.Queue;

                Statistics = new[] {state.Statistics};
                return state;
            }

            private bool CheckWorkerAbort(SolverCommand arg)
            {
                return !Worker.OwnerState.IsRunning;
            }
        }


      

       
    }
}