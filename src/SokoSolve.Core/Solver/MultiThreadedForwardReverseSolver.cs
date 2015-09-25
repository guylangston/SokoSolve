using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Sokoban.Core.Debugger;

namespace Sokoban.Core.Solver
{
    public class MultiThreadedForwardReverseSolver : ISolver
    {

        public virtual int VersionMajor { get { return 2; } }
        public virtual int VersionMinor { get { return 1; } }
        public int VersionUniversal { get { return SolverHelper.VersionUniversal; } }

        

        public virtual string VersionDescription
        {
            get
            {
                return "Multi-threaded logic for solving a set of Reverse and a set of Forward streams on a SINGLE pool";
            }
        }
        public abstract class Worker
        {
            public INodeEvaluator Evaluator { get; set; }
            public ISolverQueue Queue { get; set; }
            public ISolverNodeLookup Pool { get; set; }
            public ISolverNodeLookup PoolSolution { get; set; }
            public string Name { get; set; }
            public SolverCommand Command { get; set; }
            public SolverCommandResult WorkerCommandResult { get; set; }
            public ISolver Solver { get; set; }
            public Task<Worker> Task { get; set; }
            public MultiThreadedForwardReverseSolver Owner { get; set; }

            public MultiThreadedForwardReverseSolver.CommandResult OwnerState { get; set; }
            public Thread Thread { get; set; }
            public abstract void Init();
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
                Solver = new ForwardSolver()
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
                Solver = new ReverseSolver()
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

                state.Command.Parent = Worker.Owner;
                state.Command.CheckAbort = x => !Worker.OwnerState.IsRunning;
                
                state.Statistics.Name = GetType().Name;
                state.Pool = Worker.Pool;

                state.Evaluator = new ForwardEvaluator();
                state.Queue = Worker.Queue;

                Statistics = new SolverStatistics[] { state.Statistics};
                return state;
            }

        }

        private class ReverseSolver : SingleThreadedReverseSolver
        {
            public Worker Worker { get; set; }

            public override SolverCommandResult Init(SolverCommand command)
            {
                var state = SolverHelper.Init(new CommandResult(), command);

                state.Command.Progress = null;
                state.Command.Parent = Worker.Owner;
                state.Command.CheckAbort = CheckWorkerAbort;

                state.Statistics.Name = GetType().Name; ;
                state.Pool = Worker.Pool;

                state.Evaluator = new ReverseEvaluator();
                state.Queue = Worker.Queue;

                Statistics = new SolverStatistics[] { state.Statistics };
                return state;
            }

            private bool CheckWorkerAbort(SolverCommand arg)
            {
                return !Worker.OwnerState.IsRunning;
            }
        }


        

        

        public class CommandResult : SolverCommandResult
        {
            public List<Worker> Workers { get; set; }
            public  List<SolverStatistics> StatsInner { get; set; }
            public ProgressUpater Progress { get;  set; }
            public Task ProgressTask { get; set; }
            public bool IsRunning { get; set; }
            public ISolverNodeLookup PoolReverse { get; set; }
            public ISolverNodeLookup PoolForward { get; set; }
        }

        public SolverStatistics[] Statistics
        {
            get
            {
                if (current == null) return null;
                return current.StatsInner.ToArray();
            }
        }

        private CommandResult current;

        public SolverCommandResult Init(SolverCommand command)
        {
            var prog = command.Progress;
            command.Progress = null;

            var poolForward = new ThreadSafeSolverNodeLookupWrapper();
            poolForward.Statistics.Name = "Forward Pool";
            var poolReverse = new ThreadSafeSolverNodeLookupWrapper();
            poolReverse.Statistics.Name = "Reverse Pool";
            

            var queueForward = new ThreadSafeSolverQueueWrapper(new SolverQueue());
            queueForward.Statistics.Name = "Forward Queue";

            var queueReverse = new ThreadSafeSolverQueueWrapper(new SolverQueue());
            queueReverse.Statistics.Name = "Reverse Queue";
            current = new CommandResult()
            {
                PoolForward = poolForward,
                PoolReverse = poolReverse,
                Command = command,
                Statistics = new SolverStatistics()
                {
                    Name = GetType().Name,
                    Started = DateTime.Now
                },
                StatsInner = new List<SolverStatistics>(),
                Workers =  new List<Worker>
                {
                    // First Pair
                    new ForwardWorker()
                    {
                        Name = "F1",
                        Command = command,
                        Pool = poolForward,
                        PoolSolution = poolReverse,
                        Queue = queueForward
                    },
                    new ForwardWorker()
                    {
                        Name = "F2",
                        Command = command,
                        Pool = poolForward,
                        PoolSolution = poolReverse,
                        Queue = queueForward
                    },
                    new ForwardWorker()
                    {
                        Name = "F3",
                        Command = command,
                        Pool = poolForward,
                        PoolSolution = poolReverse,
                        Queue = queueForward
                    },
                    new ForwardWorker()
                    {
                        Name = "F4",
                        Command = command,
                        Pool = poolForward,
                        PoolSolution = poolReverse,
                        Queue = queueForward
                    },

                    new ReverseWorker()
                    {
                        Name = "R1",
                        Command = command,
                        Pool = poolReverse,
                        PoolSolution = poolForward,
                        Queue =queueReverse
                    },
                    new ReverseWorker()
                    {
                        Name = "R2",
                        Command = command,
                        Pool = poolReverse,
                        PoolSolution = poolForward,
                        Queue = queueReverse
                    },
                    new ReverseWorker()
                    {
                        Name = "R3",
                        Command = command,
                        Pool = poolReverse,
                        PoolSolution = poolForward,
                        Queue = queueReverse
                    },
                    new ReverseWorker()
                    {
                        Name = "R4",
                        Command = command,
                        Pool = poolReverse,
                        PoolSolution = poolForward,
                        Queue = queueReverse
                    }
                },
            };

            current.StatsInner.Add(current.Statistics);
            current.StatsInner.Add(poolForward.Statistics);
            current.StatsInner.Add(poolReverse.Statistics);
            current.StatsInner.Add(current.Statistics);
            current.StatsInner.Add(queueForward.Statistics);
            current.StatsInner.Add(queueReverse.Statistics);
            
            var tmp = command.Progress;
            command.Progress = null;
            foreach (var worker in current.Workers)
            {
                worker.Owner = this;
                worker.OwnerState = current;
                worker.Init();
                if (worker.Solver.Statistics != null)
                {
                    current.StatsInner.AddRange(worker.Solver.Statistics);
                }
                worker.Task = new Task<Worker>(x => Execute((Worker)x), worker, TaskCreationOptions.LongRunning);
            }
            command.Progress = tmp;

            // Init queues
            current.Workers.First(X => X.Evaluator is ForwardEvaluator).Evaluator.Init(command.Puzzle, queueForward);
            current.Workers.First(X => X.Evaluator is ReverseEvaluator).Evaluator.Init(command.Puzzle, queueReverse);

            current.Progress = new ProgressUpater(this, current);
            current.ProgressTask = new Task(current.Progress.Worker);

          

            command.Progress = prog;

            return current;
        }

        public  class ProgressUpater
        {
            private readonly MultiThreadedForwardReverseSolver solver;
            private readonly CommandResult commandResult;

            public ProgressUpater(MultiThreadedForwardReverseSolver solver, CommandResult commandResult)
            {
                this.solver = solver;
                this.commandResult = commandResult;
            }

            public void Worker()
            {
                int last = 0;
                while (commandResult.IsRunning)
                {
                    Thread.Sleep(1000);

                    commandResult.Statistics.TotalNodes = commandResult.PoolForward.Statistics.TotalNodes +
                                                          commandResult.PoolReverse.Statistics.TotalNodes;

                    var delta = commandResult.Statistics.TotalNodes - last;
                    commandResult.Statistics.Text = string.Format("Delta: {0}", delta);

                    commandResult.Command.Progress.Update(solver, commandResult, commandResult.Statistics);
                    last = commandResult.Statistics.TotalNodes;

                }
            
            }
        }

        public void Solve(SolverCommandResult state)
        {
            var full = (CommandResult)state;
            full.IsRunning = true;
            full.ProgressTask.Start();

            foreach (var worker in full.Workers)
            {
                worker.Task.Start();
            }


            var allTasks = full.Workers.Select(x => x.Task).ToArray();
            Task.WaitAll(allTasks);

            full.IsRunning = false;

            foreach (var worker in full.Workers)
            {
                if (worker.WorkerCommandResult.HasSolution)
                {
                    if (worker.WorkerCommandResult.Solutions != null)
                    {
                        if (full.Solutions == null) full.Solutions = new List<SolverNode>();
                        full.Solutions.AddRange(worker.WorkerCommandResult.Solutions);
                    }

                    if (worker.WorkerCommandResult.SolutionsWithReverse != null)
                    {
                        if (full.SolutionsWithReverse == null) full.SolutionsWithReverse = new List<SolutionChain>();
                        full.SolutionsWithReverse.AddRange(worker.WorkerCommandResult.SolutionsWithReverse);
                    }

                    
                }
            }

            state.Statistics.Completed = DateTime.Now;
        }

        private Worker Execute(Worker worker)
        {
            try
            {
                worker.Solve();
                if (worker.WorkerCommandResult.HasSolution && worker.OwnerState.Command.ExitConditions.StopOnSolution)
                {
                    worker.OwnerState.IsRunning = false;
                }
            }
            catch (Exception  ex)
            {
                worker.WorkerCommandResult.Exception = ex;
            }
            return worker;
        }
    }
}