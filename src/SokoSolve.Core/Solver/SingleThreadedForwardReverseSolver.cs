using System;
using System.Collections.Generic;
using SokoSolve.Core.Analytics;

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
        
        public SolverStatistics[]? Statistics { get; protected set; }
        public string                                  TypeDescriptor                                 => null;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverCommandResult state) => throw new NotSupportedException();

        public virtual SolverCommandResult Init(SolverCommand command)
        {
            var state = new CommandResult
            {
                Command = command,
                SolutionsNodes = new List<SolverNode>(),
                SolutionsNodesReverse = new List<SolutionChain>(),
                Forward = new SolverData
                {
                    Evaluator = new ForwardEvaluator(),
                    Queue = new SolverQueue(),
                    PoolForward = new SolverNodeLookupByBucket()
                },
                Reverse = new SolverData
                {
                    Evaluator = new ReverseEvaluator(),
                    Queue = new SolverQueue(),
                    PoolReverse = new SolverNodeLookupByBucket()
                }
            };
            state.Forward.PoolReverse = state.Reverse.PoolReverse;
            state.Reverse.PoolForward = state.Forward.PoolForward;

            state.StaticMaps = new StaticAnalysisMaps(command.Puzzle);
            
            state.Forward.Root = state.Forward.Evaluator.Init(command.Puzzle, state.Forward.Queue);
            state.Reverse.Root = state.Reverse.Evaluator.Init(command.Puzzle, state.Reverse.Queue);
            Statistics = new[]
            {
                state.Statistics,
                state.Forward.PoolForward.Statistics,
                state.Reverse.PoolReverse.Statistics,
                state.Forward.Queue.Statistics,
                state.Reverse.Queue.Statistics
            };

            return state;
        }

        public IEnumerable<(string name, string text)> GetSolverDescriptionProps(SolverCommandResult state)
        {
            if (state is CommandResult res)
            {
                yield return ("Pool.Forward", res.Forward.PoolForward?.GetType().Name);
                yield return ("Queue.Forward", res.Forward.Queue?.GetType().Name);
                yield return ("Pool.Reverse", res.Reverse.PoolReverse?.GetType().Name);
                yield return ("Queue.Reverse", res.Reverse.Queue?.GetType().Name);
            }
            else
            {
                throw new Exception();
            }
            
        }

        public void Solve(SolverCommandResult state)
        {
            Solver((CommandResult) state);
        }


        

        public void Solver(CommandResult state)
        {
            if (state == null) throw new ArgumentNullException("state");
            const int tick = 1000;
            
            state.Statistics.TotalNodes = 0;
            while (true)
            {
                var res = DequeueAndEval(state, state.Forward, state.Forward.PoolForward, state.Forward.PoolReverse);
                if (!res) break;
                
                res = DequeueAndEval(state, state.Reverse, state.Reverse.PoolReverse, state.Reverse.PoolForward);
                if (!res) break;
                
                if (state.Statistics.TotalNodes % tick == 0 && CheckExit(state)) break;
                        
            }
            
            SolverHelper.GetSolutions(state, true);

        }

        private bool CheckExit(CommandResult state)
        {
            var check = state.Command.ExitConditions.ShouldExit(state);
            if (check != ExitConditions.Conditions.Continue)
            {
                state.EarlyExit = true;
                state.Statistics.Completed = DateTime.Now;
                state.Exit = check;
                {
                    return true;
                }
            }

            return false;
        }

        private bool DequeueAndEval(CommandResult state, SolverData part, ISolverNodeLookup pool,
            ISolverNodeLookup solution)
        {
            var node = part.Queue.Dequeue();
            if (node == null) return false;

            if (part.Evaluator.Evaluate(state, part.Queue, pool, solution, node))
                // Solution
                if (state.Command.ExitConditions.StopOnSolution)
                    return false;
            state.Statistics.TotalNodes++;
            return true;
        }

        public class SolverData
        {
            public SolverNode? Root { get; set; }
            public ISolverQueue? Queue { get; set; }
            public INodeEvaluator? Evaluator { get; set; }
            public ISolverNodeLookup? PoolForward { get; set; }
            public ISolverNodeLookup? PoolReverse { get; set; }
        }

        public class CommandResult : SolverCommandResult
        {
            public SolverData? Forward { get; set; }
            public SolverData? Reverse { get; set; }
        }
    }
}