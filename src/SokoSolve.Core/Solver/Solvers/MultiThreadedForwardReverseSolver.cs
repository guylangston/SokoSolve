using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Solver.Lookup;
using SokoSolve.Core.Solver.Queue;
using TextRenderZ;

namespace SokoSolve.Core.Solver.Solvers
{
    
    public class MultiThreadedForwardReverseSolver : ISolver
    {
        private readonly ISolverNodePoolingFactory nodePoolingFactory;
        //private SolverStateMultiThreaded? masterState = null;

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
            $"Multi-threaded logic for solving a set of Forward (T:{ThreadCountForward}) and a set of Reverse (T:{ThreadCountForward}) streams on a SINGLE pool";

       

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
            if (!poolForward.IsThreadSafe) throw new NotSupportedException(poolForward.GetType().Name);
            
            var poolReverse = command.ServiceProvider.GetInstanceElseDefault<INodeLookup>(
                () => {
                    var def = new NodeLookupSlimRwLock(new NodeLookupBinarySearchTree(new NodeLookupLongTerm()));
                    command.Report?.WriteLine($"ServiceProvider does not contain ISolverPool; using default => {def.TypeDescriptor}");
                    return def;
                });
            if (!poolReverse.IsThreadSafe) throw new NotSupportedException(poolReverse.GetType().Name);
            poolForward.Statistics.Name  = "Pool (Forward)";
            poolReverse.Statistics.Name  = "Pool (Reverse)";
            
            var queueForward = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(
                () => {
                    var def = new SolverQueueSortedWithDeDup();
                    command.Report?.WriteLine($"ServiceProvider does not contain ISolverQueue; using default => {def.TypeDescriptor}");
                    return def;
                });
            if (!queueForward.IsThreadSafe) throw new NotSupportedException(queueForward.GetType().Name);
            var queueReverse = command.ServiceProvider.GetInstanceElseDefault<ISolverQueue>(
                () => {
                    var def = new SolverQueueSortedWithDeDup();
                    command.Report?.WriteLine($"ServiceProvider does not contain ISolverQueue; using default => {def.TypeDescriptor}");
                    return def;
            });
            if (!queueReverse.IsThreadSafe) throw new NotSupportedException(queueReverse.GetType().Name);
            queueForward.Statistics.Name = "Queue (Forward)";
            queueReverse.Statistics.Name = "Queue (Reverse)";
            


            var fwdEvalMaster = new ForwardEvaluator(command, nodePoolingFactory);
            var fwdRoot       = (SolverNodeRoot)fwdEvalMaster.Init(command.Puzzle, queueForward);
            var fwdTree       = new TreeStateCore(fwdRoot, poolForward, queueForward);

            var revEvalMaster = new ReverseEvaluator(command, nodePoolingFactory);
            var revRoot       = (ReverseEvaluator.SolverNodeRootReverse)revEvalMaster.Init(command.Puzzle, queueReverse);
            var revTree       = new TreeStateCore(revRoot, poolReverse, queueReverse);

            fwdTree.Alt = revTree;
            revTree.Alt = fwdTree;
            
            var masterState = new SolverStateMultiThreaded(command, this, fwdTree, revTree)
            {
                StaticMaps = new StaticAnalysisMaps(command.Puzzle)
            };
            
            // Common Init
            SolverBase<SolverStateMultiThreaded>.InitState(command, masterState);
            
          
            masterState.GlobalStats.Name    = GetType().Name;
            masterState.GlobalStats.Started = DateTime.Now;
            
            queueForward.Init(masterState, SolverQueueMode.QueueAndUnEvalLookup);
            queueReverse.Init(masterState, SolverQueueMode.QueueAndUnEvalLookup);

            
            for (int i = 0; i < ThreadCountForward; i++)
            {
                var primary = new TreeState(
                    fwdTree.Root, 
                    fwdTree.Pool, 
                    new SolverQueueLocalSlave(fwdTree.Queue),               // Keep a 1:1 between eval and queue (each eval has its own queue)
                    new ForwardEvaluator(command, nodePoolingFactory));
                var forwardWorker = new SingleThreadWorker(
                    $"F{i,00}",
                    nodePoolingFactory,
                    this,
                    masterState,
                    new SolverStateMultiThreaded.WorkerState(masterState, primary));

                forwardWorker.Solver = new ForwardSolver(command, nodePoolingFactory, forwardWorker);
                
                // Use Threads instead of Tasks to get explicit control
                // Also stops .net trying to auto-level CPU demand across other threads
                forwardWorker.Thread = new Thread(x => Execute((SingleThreadWorker)x))
                {
                    Name         = forwardWorker.Name,
                    Priority     = ThreadPriority.Normal,
                    IsBackground = false
                };

                masterState.Workers.Add(forwardWorker);
            }
            for (int i = 0; i < ThreadCountReverse; i++)
            {
                var primary = new TreeState(
                    revTree.Root, 
                    revTree.Pool, 
                    new SolverQueueLocalSlave(revTree.Queue),               // Keep a 1:1 between eval and queue (each eval has its own queue)
                    new ReverseEvaluator(command, nodePoolingFactory));

                var reverseWorker = new SingleThreadWorker($"R{i,00}", nodePoolingFactory,
                    this, masterState,
                    new SolverStateMultiThreaded.WorkerState(masterState, primary));
                
                reverseWorker.Solver = new ReverseSolver(command, nodePoolingFactory, reverseWorker);
                
                // Use Threads instead of Tasks to get explicit control
                // Also stops .net trying to auto-level CPU demand across other threads
                reverseWorker.Thread = new Thread(x => Execute((SingleThreadWorker)x))
                {
                    Name         = reverseWorker.Name,
                    Priority     = ThreadPriority.Normal,
                    IsBackground = false
                };
               
                masterState.Workers.Add(reverseWorker);
            }

            masterState.Statistics.Add(masterState.GlobalStats);
            masterState.Statistics.Add(poolForward.Statistics);
            masterState.Statistics.Add(poolReverse.Statistics);
            masterState.Statistics.Add(queueForward.Statistics);
            masterState.Statistics.Add(queueReverse.Statistics);
            
            // Init queues
            //if (queueForward is ReuseTreeSolverQueue tqf) tqf.Root = masterState.Root;
            //if (queueReverse is ReuseTreeSolverQueue tqr) tqr.Root = masterState.RootReverse;
            
            return masterState;
        }
        
        
        private ProcessThread? GetThreadProc(Process proc, Thread thread)
        {
            foreach (ProcessThread processThread in proc.Threads)
            {
                if (processThread.Id == thread.ManagedThreadId)
                {
                    return processThread;
                }
            }
            return null;
        }


        public ExitResult Solve(SolverState state)
        {
            var masterState = (SolverStateMultiThreaded) state;    
            if (masterState.Workers is null || masterState.Workers.Count == 0)
            {
                throw new Exception("Init did not create workers");
            }
            
            var allTasksArray = masterState.Workers.Select(x=>x.Thread).ToArray();
            var cancel = state.Command.CancellationSource;
            if (cancel.IsCancellationRequested) throw new InvalidDataException();
            
            // Start and wait
            masterState.IsRunning = true;
            
            // TODO: Confirm the queue is NOT empty
            var x = masterState.Forward.Queue;
            foreach (var worker in masterState.Workers)
            {
                if (worker.Thread == null) throw new ArgumentNullException(nameof(worker.Thread));
                worker.Thread.Start(worker);
            }
            
            Task statisticsTick = null;
            if (state.Command.AggProgress != null)
            {
                // Setup global/aggregate statistics and updates
                statisticsTick = Task.Run(() =>
                {
                    while (masterState.IsRunning)
                    {
                        Thread.Sleep(1000);
                        state.GlobalStats.TotalNodes = masterState.Forward.Pool.Statistics.TotalNodes 
                                                      + masterState.Reverse.Pool.Statistics.TotalNodes;

                        state.GlobalStats.Warnings = masterState.Workers.Sum(x => x.WorkerState.GlobalStats.Warnings);
                        state.GlobalStats.Errors = masterState.Workers.Sum(x => x.WorkerState.GlobalStats.Errors);

                        var qf = masterState.Forward.Queue.Statistics.CurrentNodes * 100f / masterState.Forward.Pool.Statistics.TotalNodes;
                        var qr = masterState.Reverse.Queue.Statistics.CurrentNodes * 100f / masterState.Reverse.Pool.Statistics.TotalNodes;
                        
                        var txt = new FluentString()
                          .Append($"==> {state.GlobalStats.ToString(false, true)}")
                          .Append($" Fwd({masterState.Forward.Pool.Statistics.TotalNodes:#,##0} q{qf:#,##0.0}%)")
                          .Append($" Rev({masterState.Reverse.Pool.Statistics.TotalNodes:#,##0} q{qr:#,##0.0}%)")
                          .IfNotNull(masterState.KnownSolutionTracker, tracker => "; " +tracker!.Status )
                          ;
                        state.Command.AggProgress.Update(this, state, state.GlobalStats, txt);
                        
                    }
                    if (state.Command.AggProgress is IDisposable dp) dp.Dispose();
                    
                });    
            }

            if (WaitForAll(allTasksArray, state.Command.ExitConditions.Duration.TotalMilliseconds, cancel.Token))
            {
                // Close down the workers as gracefully as possible
                state.Command.ExitConditions.ExitRequested = true;
                state.Command.CancellationSource.Cancel(true);
                
                Console.WriteLine("Timeout... Cancelling");
                
                // Allow them to process the ExitRequested
                Thread.Sleep((int)TimeSpan.FromSeconds(1).TotalMilliseconds);
                
                // Close down any outliers
                var wait = masterState.Workers.Any(x => x.IsRunning);
                if (wait)
                {
                    Console.WriteLine("Waiting.... For Task to complete (after cancellation)");
                    Thread.Sleep((int)TimeSpan.FromSeconds(5).TotalMilliseconds);

                    var notFinished = masterState.Workers.Where(x => x.IsRunning);
                    if (notFinished.Any())
                    {
                        throw new AggregateException("Workers did not exit");
                    }
                }
                
                state.Exit = ExitResult.TimeOut;
            }
            
            // Cleanup
            foreach (var worker in masterState.Workers)
            {
                if (worker.IsRunning) worker.Thread.Abort();
            }
            
            masterState.IsRunning = false;
            statisticsTick?.Wait();
            // Update stats
            state.GlobalStats.Completed = DateTime.Now;
            state.GlobalStats.TotalNodes = masterState.Forward.Pool.Statistics.TotalNodes 
                                           + masterState.Reverse.Pool.Statistics.TotalNodes;
            foreach (var stat in masterState.Statistics)
            {
                stat.Completed = state.GlobalStats.Completed;
            }

            // Get solutions & Exit Conditions & Errors
            var errors = masterState.Workers.Select(x => x.WorkerState.Exception).Where(x => x != null).ToList();
            if (errors.Any())
            {
                throw new AggregateException(errors);
            }
            foreach (var worker in masterState.Workers)
            {
                worker.WorkerState.GlobalStats.Completed = state.GlobalStats.Completed;

                if (worker.WorkerState.HasSolution)
                {
                    if (worker.WorkerState.SolutionsNodes?.Count > 0)
                    {
                        masterState.SolutionsNodes.AddRange(worker.WorkerState.SolutionsNodes);
                    }

                    if (worker.WorkerState.SolutionsChains?.Count > 0)
                    {
                        masterState.SolutionsChains.AddRange(worker.WorkerState.SolutionsChains);
                    }

                    if (worker.WorkerState.Solutions?.Count > 0)
                    {
                        masterState.Solutions.AddRange(worker.WorkerState.Solutions);
                    }
                    
                }
            }


            // Update the parent state's exit result
            if (state.Exit == ExitResult.Continue)
            {
                state.Exit = SetParentExitStatus(state, masterState);
            }
            
            return state.Exit;
        }
        
        
        private static bool WaitForAll(Thread[] allTasksArray, double durationMs, CancellationToken cancelToken)
        {
            var timer = new Stopwatch();
            timer.Start();
            while (allTasksArray.Any(x => x.IsAlive))
            {
                if (timer.ElapsedMilliseconds > durationMs)
                {
                    return false;
                }
                Thread.Sleep(500);
            }
            return false;
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

        private void Execute(SingleThreadWorker worker)
        {
            try
            {
                Thread.BeginThreadAffinity();
                worker.IsRunning = true;
                
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
                worker.IsRunning = false;
                Thread.EndThreadAffinity();
            }
            
        }

        public string TypeDescriptor => $"{GetType().Name}:fr! ==> {nodePoolingFactory}";
        public IEnumerable<(string name, string? text)> GetTypeDescriptorProps(SolverState state)
        {
            yield return ("Strategy.ShortName", "fr!");
            if (state is SolverStateMultiThreaded cc)
            {
                yield return ("Forward.Pool", cc.Forward?.Pool.TypeDescriptor);
                yield return ("Forward.Queue", cc.Forward?.Queue.TypeDescriptor);
                yield return ("Reverse.Pool", cc.Reverse?.Pool.TypeDescriptor);
                yield return ("Reverse.Queue", cc.Reverse?.Queue.TypeDescriptor);

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

        public class SingleThreadWorker
        {
            public SingleThreadWorker(string name, ISolverNodePoolingFactory poolingFactory, 
                MultiThreadedForwardReverseSolver owner, 
                SolverStateMultiThreaded ownerState, 
                SolverStateMultiThreaded.WorkerState workerState)
            {
                Name           = name;
                PoolingFactory = poolingFactory;
                Owner          = owner;
                OwnerState     = ownerState;
                WorkerState    = workerState;

                WorkerState.GlobalStats.Name = name;
            }


            public SolverBase<SolverStateMultiThreaded.WorkerState> Solver         { get; set; }
            public string                                           Name           { get; }
            public ISolverNodePoolingFactory                        PoolingFactory { get; }
            public MultiThreadedForwardReverseSolver                Owner          { get; }
            public SolverStateMultiThreaded                         OwnerState     { get; }
            public SolverStateMultiThreaded.WorkerState             WorkerState    { get; }
            public Thread                                           Thread         { get; set; }
            public bool                                             IsRunning      { get; set; }

            public void Solve() => Solver.Solve(WorkerState);
        }

        

        private class ForwardSolver : SolverBase<SolverStateMultiThreaded.WorkerState>
        {
            private readonly ISolverNodePoolingFactory nodePoolingFactory;

            public ForwardSolver(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory, SingleThreadWorker worker) 
            : base(2,0, $"Forward-component")
            {
                this.nodePoolingFactory = nodePoolingFactory;
                Worker = worker ?? throw new ArgumentNullException(nameof(worker));
            }

            public SingleThreadWorker Worker { get;  }

            public override ExitResult Solve(SolverState state)
                => SolveInner(Worker.WorkerState, Worker.WorkerState.Primary);
            
            public override SolverStateMultiThreaded.WorkerState Init(SolverCommand command)
            {
                return Worker.WorkerState;
            }
        }

        private class ReverseSolver : SolverBase<SolverStateMultiThreaded.WorkerState>
        {
            private readonly ISolverNodePoolingFactory nodePoolingFactory;

            public ReverseSolver(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory, SingleThreadWorker worker) 
                : base(2,0, $"Forward-component")
            {
                this.nodePoolingFactory = nodePoolingFactory;
                Worker = worker ?? throw new ArgumentNullException(nameof(worker));
            }

            public SingleThreadWorker Worker { get;  }
            
            public override ExitResult Solve(SolverState state)
                => SolveInner(Worker.WorkerState, Worker.WorkerState.Primary);

            public override SolverStateMultiThreaded.WorkerState Init(SolverCommand command)
            {
                return Worker.WorkerState;
            }
        }


      

       
    }
}