using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SokoSolve.Core.Analytics;
using TextRenderZ;
using Path=SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    
    
    
    public class MultiThreadedForwardReverseSolver : ISolver
    {
        private readonly ISolverNodePoolingFactory nodePoolingFactory;
        private MultiThreadedSolverState? masterState = null;

        public MultiThreadedForwardReverseSolver(ISolverNodePoolingFactory nodePoolingFactory)
        {
            this.nodePoolingFactory = nodePoolingFactory;
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

        public string TypeDescriptor => $"{GetType().Name}:fr! ==> {nodePoolingFactory}";
        public IEnumerable<(string name, string? text)> GetTypeDescriptorProps(SolverState state)
        {
            yield return ("Strategy.ShortName", "fr!");
            if (state is MultiThreadedSolverState cc)
            {
                yield return ("Pool.Forward", cc.PoolForward?.TypeDescriptor);
                yield return ("Pool.Reverse", cc.PoolReverse?.TypeDescriptor);
                yield return ("Queue.Forward", cc.QueueForward?.TypeDescriptor);
                yield return ("Queue.Reverse", cc.QueueReverse?.TypeDescriptor);
                yield return ("NodeFactory", nodePoolingFactory.TypeDescriptor);
                if (nodePoolingFactory.GetTypeDescriptorProps(state) != null)
                {
                    foreach (var ip in nodePoolingFactory.GetTypeDescriptorProps(state))
                    {
                        yield return ip;
                    }
                }
            }
            else
            {
                throw new InvalidDataException(state?.GetType().Name);
            }
        }

        public SolverState Init(SolverCommand command)
        {
            if (nodePoolingFactory is ISolveNodePoolingFactoryPuzzleDependant dep)
            {
                dep.SetupForPuzzle(command.Puzzle);
            }

            if (command.ServiceProvider == null)
            {
                throw new Exception("Must have a non-null ServiceProvider");
            }

            var poolForward = command.ServiceProvider.GetInstanceElseDefault<INodeLookup>(
                () => {
                    var def = new NodeLookupSlimRwLock(new NodeLookupBinarySearchTree(new NodeLookupLongTerm()));
                    command.Report?.WriteLine($"ServiceProvider does not contain ISolverPool; using default => {def.TypeDescriptor}");
                    return def;
                });
            var poolReverse = command.ServiceProvider.GetInstanceElseDefault<INodeLookup>(
                () => {
                    var def = new NodeLookupSlimRwLock(new NodeLookupBinarySearchTree(new NodeLookupLongTerm()));
                    command.Report?.WriteLine($"ServiceProvider does not contain ISolverPool; using default => {def.TypeDescriptor}");
                    return def;
                });
            poolForward.Statistics.Name  = "Pool (Forward)";
            poolReverse.Statistics.Name  = "Pool (Reverse)";
            
            var queueForward = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(
                () => {
                    var def = new SolverQueueConcurrent();
                    command.Report?.WriteLine($"ServiceProvider does not contain ISolverQueue; using default => {def.TypeDescriptor}");
                    return def;
                });
            var queueReverse = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(
                () => {
                    command.Report?.WriteLine("ServiceProvider does not contain ISolverQueue; using default");
                    return new SolverQueueConcurrent();
            });
            queueForward.Statistics.Name = "Queue (Forward)";
            queueReverse.Statistics.Name = "Queue (Reverse)";


            masterState = new MultiThreadedSolverState(command, this)
            {
                PoolForward = poolForward,
                PoolReverse = poolReverse,
                QueueForward = queueForward,
                QueueReverse = queueReverse,
                
                StatsInner = new List<SolverStatistics>(),
                Workers    = new List<Worker>(),
                StaticMaps = new StaticAnalysisMaps(command.Puzzle)
            };

            masterState.GlobalStats.Name    = GetType().Name;
            masterState.GlobalStats.Started = DateTime.Now;

            for (int i = 0; i < ThreadCountForward; i++)
            {
                masterState.Workers.Add(new ForwardWorker(nodePoolingFactory)
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
                masterState.Workers.Add(new ReverseWorker(nodePoolingFactory)
                {
                    Name         = $"R{i,00}",
                    Command      = command,
                    Pool         = poolReverse,
                    PoolSolution = poolForward,
                    Queue        = queueReverse
                });
            }

            masterState.StatsInner.Add(masterState.GlobalStats);
            masterState.StatsInner.Add(poolForward.Statistics);
            masterState.StatsInner.Add(poolReverse.Statistics);
            masterState.StatsInner.Add(queueForward.Statistics);
            masterState.StatsInner.Add(queueReverse.Statistics);

            foreach (var worker in masterState.Workers)
            {
                worker.Owner = this;
                worker.OwnerState = masterState;
                worker.Init();
                worker.WorkerState.ParentState = masterState;
                
                worker.Task = new Task<Worker>(
                    x => Execute((Worker) x), 
                    worker, 
                    command.CancellationSource.Token,
                    TaskCreationOptions.LongRunning);
            }

            // Init queues
            if (ThreadCountForward > 0)
            {
                var firstForward = masterState.Workers.First(x => x.Evaluator is ForwardEvaluator);
                masterState.Root = firstForward.Evaluator.Init(command.Puzzle, queueForward);
                masterState.PoolForward.Add(masterState.Root.Recurse().ToList());
                
                if (queueForward is ReuseTreeSolverQueue tqf) tqf.Root = masterState.Root;
            }

            if (ThreadCountReverse > 0)
            {
                var firstReverse = masterState.Workers.First(x => x.Evaluator is ReverseEvaluator);
                masterState.RootReverse = firstReverse.Evaluator.Init(command.Puzzle, queueReverse);
                masterState.PoolReverse.Add(masterState.RootReverse.Recurse().ToList());
                
                if (queueReverse is ReuseTreeSolverQueue tqr) tqr.Root = masterState.RootReverse;
            }
            return masterState;
        }

     
        public ExitResult Solve(SolverState state)
        {
            var full     = (MultiThreadedSolverState) state;    
            if (full.Workers is null || full.Workers.Count == 0)
            {
                throw new Exception("Init did not create workers");
            }
            
            var allTasksArray = full.Workers.Select(x=>(Task)x.Task).ToArray();
            var cancel = state.Command.CancellationSource;
            if (cancel.IsCancellationRequested) throw new InvalidDataException();
            
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
                        state.GlobalStats.TotalNodes = masterState.PoolForward.Statistics.TotalNodes 
                                                      + masterState.PoolReverse.Statistics.TotalNodes;

                        state.GlobalStats.Warnings = masterState.Workers.Sum(x => x.WorkerState.GlobalStats.Warnings);
                        state.GlobalStats.Errors = masterState.Workers.Sum(x => x.WorkerState.GlobalStats.Errors);
                        
                        var txt = new FluentString()
                          .Append($"==> {state.GlobalStats.ToString(false, true)}")
                          .Append($" Fwd({masterState.PoolForward.Statistics.TotalNodes:#,##0} q{masterState.QueueForward.Statistics.TotalNodes:#,##0})")
                          .Append($" Rev({masterState.PoolReverse.Statistics.TotalNodes:#,##0} q{masterState.QueueReverse.Statistics.TotalNodes:#,##0})");
                        state.Command.AggProgress.Update(this, state, state.GlobalStats, txt);
                        
                    }
                    if (state.Command.AggProgress is IDisposable dp) dp.Dispose();
                    
                });    
            }

            if (!Task.WaitAll(allTasksArray, (int) state.Command.ExitConditions.Duration.TotalMilliseconds, cancel.Token))
            {
                // Close down the workers as gracefully as possible
                state.Command.ExitConditions.ExitRequested = true;
                state.Command.CancellationSource.Cancel(true);
                
                Console.WriteLine("Timeout... Cancelling");
                
                // Allow them to process the ExitRequested
                Thread.Sleep((int)TimeSpan.FromSeconds(1).TotalMilliseconds);
                
                // Close down any outliers
                var wait = full.Workers.Any(x => x.Task.Status == TaskStatus.Running);
                if (wait)
                {
                    Console.WriteLine("Waiting.... For Task to complete (after cancellation)");
                    Thread.Sleep((int)TimeSpan.FromSeconds(5).TotalMilliseconds);

                    var notFinished = full.Workers.Where(x => x.Task.Status == TaskStatus.Running);
                    if (notFinished.Any())
                    {
                        throw new AggregateException("Workers did not exit");
                    }
                }
                
                state.Exit = ExitResult.TimeOut;
            }
            
            // Cleanup
            foreach (var worker in full.Workers)
            {
                worker.Task.Dispose();
            }
            
            full.IsRunning = false;
            statisticsTick?.Wait();
            // Update stats
            state.GlobalStats.Completed = DateTime.Now;
            state.GlobalStats.TotalNodes = masterState.PoolForward.Statistics.TotalNodes 
                                           + masterState.PoolReverse.Statistics.TotalNodes;
            foreach (var stat in masterState.StatsInner)
            {
                stat.Completed = state.GlobalStats.Completed;
            }

            // Get solutions & Exit Conditions & Errors
            var errors = full.Workers.Select(x => x.WorkerState.Exception).Where(x => x != null).ToList();
            if (errors.Any())
            {
                throw new AggregateException(errors);
            }
            foreach (var worker in full.Workers)
            {
                worker.WorkerState.GlobalStats.Completed = state.GlobalStats.Completed;

                if (worker.WorkerState.HasSolution)
                {
                    if (worker.WorkerState.SolutionsNodes?.Count > 0)
                    {
                        full.SolutionsNodes ??= new List<SolverNode>();
                        full.SolutionsNodes.AddRange(worker.WorkerState.SolutionsNodes);
                    }

                    if (worker.WorkerState.SolutionsChains?.Count > 0)
                    {
                        full.SolutionsChains ??= new List<SolutionChain>();
                        full.SolutionsChains.AddRange(worker.WorkerState.SolutionsChains);
                    }

                    if (worker.WorkerState.Solutions?.Count > 0)
                    {
                        full.Solutions ??= new List<Path>();
                        full.Solutions.AddRange(worker.WorkerState.Solutions);
                    }
                    
                }
            }


            // Update the parent state's exit result
            if (state.Exit == ExitResult.Continue)
            {
                state.Exit = SetParentExitStatus(state, full);
            }
            
            
        

            return state.Exit;
        }
        
        private static ExitResult SetParentExitStatus(SolverState state, MultiThreadedSolverState full)
        {
            if (full.Workers.Any(x => x.WorkerState.Exit == ExitResult.Error))
            {
                return ExitResult.Error;
            }

            if (state.Command.ExitConditions.StopOnSolution && state.HasSolution)
            {
                return ExitResult.Solution;
            }

            var common = full.Workers.Select(x => x.WorkerState.Exit).Distinct().ToArray();
            if (common.Length == 1)
            {
                var all = common.First();
                if (all == ExitResult.QueueEmpty)
                {
                    return ExitResult.ExhaustedTree;
                }
                
                return all;
            }


            return ExitResult.Stopped;
        }

        private Worker Execute(Worker worker)
        {
            var threadid = Thread.CurrentThread.Name;
            try
            {
                Thread.CurrentThread.Name = worker.Name;
            
                worker.Solve();
                if (worker.WorkerState.HasSolution && worker.OwnerState.Command.ExitConditions.StopOnSolution)
                    worker.OwnerState.IsRunning = false;
            }
            catch (Exception ex)
            {
                worker.WorkerState.Exception = ex;
            }
            finally
            {
                //Thread.CurrentThread.Name = threadid;
            }

            return worker;
        }

        public abstract class Worker
        {
            public INodeEvaluator                    Evaluator    { get; set; }
            public ISolverQueue                      Queue        { get; set; }
            public INodeLookup                       Pool         { get; set; }
            public INodeLookup                       PoolSolution { get; set; }
            public string                            Name         { get; set; }
            public SolverCommand                     Command      { get; set; }
            public MultiThreadedSolverState          WorkerState  { get; set; }
            public ISolver                           Solver       { get; set; }
            public Task<Worker>                      Task         { get; set; }
            public MultiThreadedForwardReverseSolver Owner        { get; set; }
            public MultiThreadedSolverState          OwnerState   { get; set; }
            
            public abstract void Init();

            public void Solve() => Solver.Solve(WorkerState);
        }


        private class ForwardWorker : Worker
        {
            private readonly ISolverNodePoolingFactory nodePoolingFactory;

            public ForwardWorker(ISolverNodePoolingFactory nodePoolingFactory)
            {
                this.nodePoolingFactory = nodePoolingFactory;
            }

            public override void Init()
            {
                Evaluator = new ForwardEvaluator(Command, nodePoolingFactory);
                Solver = new ForwardSolver(Command, nodePoolingFactory, this);
                WorkerState = (MultiThreadedSolverState)Solver.Init(Command);
            }
        }

        private class ReverseWorker : Worker
        {
            private readonly ISolverNodePoolingFactory nodePoolingFactory;

            public ReverseWorker(ISolverNodePoolingFactory nodePoolingFactory)
            {
                this.nodePoolingFactory = nodePoolingFactory;
            }

            public override void Init()
            {
                Evaluator   = new ReverseEvaluator(Command, nodePoolingFactory);
                Solver      = new ReverseSolver(Command, nodePoolingFactory, this);
                WorkerState = (MultiThreadedSolverState)Solver.Init(Command);
            }
        }

        private class ForwardSolver : SingleThreadedForwardSolver
        {
            private readonly ISolverNodePoolingFactory nodePoolingFactory;

            public ForwardSolver(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory, Worker worker) : base(cmd, nodePoolingFactory)
            {
                this.nodePoolingFactory = nodePoolingFactory;
                Worker = worker;
            }

            public Worker Worker { get; set; }

            public override SolverState Init(SolverCommand command)
            {
                var state = SolverHelper.Init(new MultiThreadedSolverState(command, this), command);
                state.Command.Parent  = Worker.Owner;
                state.GlobalStats.Name = $"{GetType().Name}:{Worker.Name}";
                
                state.Evaluator   = new ForwardEvaluator(command, nodePoolingFactory);
                state.Pool        = Worker.Pool;
                state.PoolReverse = Worker.PoolSolution; 
                state.Queue       = Worker.Queue;

                
                return state;
            }
        }

        private class ReverseSolver : SingleThreadedReverseSolver
        {
            private readonly ISolverNodePoolingFactory nodePoolingFactory;

            public ReverseSolver(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory, Worker? worker) : base(cmd, nodePoolingFactory)
            {
                this.nodePoolingFactory = nodePoolingFactory;
                Worker = worker;
            }

            public Worker? Worker { get; set; }

            public override SolverState Init(SolverCommand command)
            {
                var state = SolverHelper.Init(new MultiThreadedSolverState(command, this), command);
                state.Command.Parent  = Worker.Owner;
                state.GlobalStats.Name = $"{GetType().Name}:{Worker.Name}";
                state.Pool            = Worker.Pool;
                state.Evaluator       = new ReverseEvaluator(command, nodePoolingFactory);
                state.Queue           = Worker.Queue;
                
                return state;
            }

            private bool CheckWorkerAbort(SolverCommand arg)
            {
                return !Worker.OwnerState.IsRunning;
            }
        }


      

       
    }
}