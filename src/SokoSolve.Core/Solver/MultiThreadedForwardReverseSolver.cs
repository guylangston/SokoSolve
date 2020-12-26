using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Solver.Lookup;
using TextRenderZ;
using Path=SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    
    
    
    public class MultiThreadedForwardReverseSolver : ISolver
    {
        private readonly ISolverNodePoolingFactory nodePoolingFactory;
        private SolverStateMultiThreaded? masterState = null;

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
            if (state is SolverStateMultiThreaded cc)
            {
                // yield return ("Pool.Forward", cc.PoolForward?.TypeDescriptor);
                // yield return ("Pool.Reverse", cc.PoolReverse?.TypeDescriptor);
                // yield return ("Queue.Forward", cc.QueueForward?.TypeDescriptor);
                // yield return ("Queue.Reverse", cc.QueueReverse?.TypeDescriptor);
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
        
        public abstract class Worker
        {
            public string Name { get; set; }
            
            public MultiThreadedForwardReverseSolver Owner      { get; set; }
            public SolverStateMultiThreaded       OwnerState { get; set; }
            
            public SolverStateMultiThreaded.WorkerState WorkerState { get; set; }
            public ISolver                                 Solver      { get; set; }
            public Task<Worker>                            Task        { get; set; }
            

            public void Solve() => Solver.Solve(WorkerState);
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


            var fwdEvalMaster = new ForwardEvaluator(command, nodePoolingFactory);
            var fwdRoot       = (SolverNodeRoot)fwdEvalMaster.Init(command.Puzzle, queueForward);
            var fwdTree = new TreeState(fwdRoot, poolForward, queueForward);

            var revEvalMaster = new ReverseEvaluator(command, nodePoolingFactory);
            var revRoot       = (ReverseEvaluator.SolverNodeRootReverse)revEvalMaster.Init(command.Puzzle, queueForward);
            var revTree = new TreeState(revRoot, poolReverse, queueReverse);

            fwdTree.Alt = revTree;
            revTree.Alt = fwdTree;
            
            masterState = new SolverStateMultiThreaded(command, this, fwdTree, revTree)
            {
                StaticMaps = new StaticAnalysisMaps(command.Puzzle)
            };
            masterState.GlobalStats.Name    = GetType().Name;
            masterState.GlobalStats.Started = DateTime.Now;
            
            for (int i = 0; i < ThreadCountForward; i++)
            {
                var forwardWorker = new ForwardWorker(nodePoolingFactory)
                {
                    Name       = $"F{i,00}",
                    Owner      = this,
                    OwnerState = masterState,
                    
                    WorkerState = new SolverStateMultiThreaded.WorkerState(
                        masterState, command, this, new ForwardEvaluator(command, nodePoolingFactory), 
                        fwdRoot, poolForward, queueForward, 
                        poolReverse, queueReverse
                    )
                };
                forwardWorker.Solver = new ForwardSolver(command, nodePoolingFactory, forwardWorker);
                
                
                forwardWorker.Task = new Task<Worker>(
                    x => Execute((Worker) x), 
                    forwardWorker, 
                    command.CancellationSource.Token,
                    TaskCreationOptions.LongRunning);
                
                masterState.Workers.Add(forwardWorker);
            }
            for (int i = 0; i < ThreadCountReverse; i++)
            {
                var reverseWorker = new ReverseWorker(nodePoolingFactory)
                {
                    Name       = $"R{i,00}",
                    Owner      = this,
                    OwnerState = masterState,
                    WorkerState = new SolverStateMultiThreaded.WorkerState(
                        masterState, command, this, new ReverseEvaluator(command, nodePoolingFactory), 
                        revRoot, poolReverse, queueReverse
                        , poolForward, queueForward
                    )
                };
                reverseWorker.Solver = new ReverseSolver(command, nodePoolingFactory, reverseWorker);
                
                reverseWorker.Task = new Task<Worker>(
                    x => Execute((Worker) x), 
                    reverseWorker, 
                    command.CancellationSource.Token,
                    TaskCreationOptions.LongRunning);
                
                masterState.Workers.Add(reverseWorker);
            }

            masterState.StatsInner.Add(masterState.GlobalStats);
            masterState.StatsInner.Add(poolForward.Statistics);
            masterState.StatsInner.Add(poolReverse.Statistics);
            masterState.StatsInner.Add(queueForward.Statistics);
            masterState.StatsInner.Add(queueReverse.Statistics);
            
            // Init queues
            //if (queueForward is ReuseTreeSolverQueue tqf) tqf.Root = masterState.Root;
            //if (queueReverse is ReuseTreeSolverQueue tqr) tqr.Root = masterState.RootReverse;
            
            return masterState;
        }

     
        public ExitResult Solve(SolverState state)
        {
            var full     = (SolverStateMultiThreaded) state;    
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
                        full.SolutionsNodes.AddRange(worker.WorkerState.SolutionsNodes);
                    }

                    if (worker.WorkerState.SolutionsChains?.Count > 0)
                    {
                        full.SolutionsChains.AddRange(worker.WorkerState.SolutionsChains);
                    }

                    if (worker.WorkerState.Solutions?.Count > 0)
                    {
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
        
        private static ExitResult SetParentExitStatus(SolverState state, SolverStateMultiThreaded full)
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

       

        private class ForwardWorker : Worker
        {
            private readonly ISolverNodePoolingFactory nodePoolingFactory;

            public ForwardWorker(ISolverNodePoolingFactory nodePoolingFactory)
            {
                this.nodePoolingFactory = nodePoolingFactory;
            }

           
        }

        private class ReverseWorker : Worker
        {
            private readonly ISolverNodePoolingFactory nodePoolingFactory;

            public ReverseWorker(ISolverNodePoolingFactory nodePoolingFactory)
            {
                this.nodePoolingFactory = nodePoolingFactory;
            }

           
        }

        private class ForwardSolver : SolverBase<SolverStateMultiThreaded.WorkerState>
        {
            private readonly ISolverNodePoolingFactory nodePoolingFactory;

            public ForwardSolver(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory, Worker worker) 
            {
                this.nodePoolingFactory = nodePoolingFactory;
                Worker = worker;
            }

            public Worker Worker { get; set; }

            public override SolverStateMultiThreaded.WorkerState InitInner(SolverCommand command)
            {
                return Worker.WorkerState;
            }
        }

        private class ReverseSolver : SolverBase<SolverStateMultiThreaded.WorkerState>
        {
            private readonly ISolverNodePoolingFactory nodePoolingFactory;

            public ReverseSolver(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory, Worker? worker) 
            {
                this.nodePoolingFactory = nodePoolingFactory;
                Worker = worker;
            }

            public Worker? Worker { get; set; }

            public override SolverStateMultiThreaded.WorkerState InitInner(SolverCommand command)
            {
                return Worker.WorkerState;
            }
        }


      

       
    }
}