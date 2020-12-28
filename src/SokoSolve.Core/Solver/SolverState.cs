using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Solver.Components;
using SokoSolve.Core.Solver.Solvers;

namespace SokoSolve.Core.Solver
{
    public abstract class SolverState
    {
        protected SolverState(SolverCommand command, ISolver solver)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Solver = solver ?? throw new ArgumentNullException(nameof(solver));
            GlobalStats = new SolverStatistics();
            Statistics  = new List<SolverStatistics>();
            SolutionsNodes = new List<SolverNode>();
            Solutions = new List<Path>();
        }

        public SolverCommand          Command         { get; }
        public ISolver                Solver          { get; }
        public SolverStatistics       GlobalStats     { get; }
        public List<SolverStatistics> Statistics      { get; }
        public List<SolverNode>       SolutionsNodes  { get; }
        public List<Path>             Solutions       { get; }
        public StaticAnalysisMaps     StaticMaps      { get; set; }
        public Exception?             Exception       { get; set; }
        public bool                   EarlyExit       { get; set; }
        public ExitResult             Exit            { get; set; }
        public SolverResultSummary?   Summary         { get; set; }
        
        // Optional Components
        public IKnownSolutionTracker? KnownSolutionTracker { get; set; }
        
        public virtual bool HasSolution =>  Solutions.Any();

        public void ThrowErrors()
        {
            if (Exception != null) throw new Exception("Solver Failed", Exception);
        }

        public abstract IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors();
    }


    public class TreeStateCore : INodeLookupReadOnly
    {
        public TreeStateCore(SolverNode root, INodeLookup pool, ISolverQueue queue)
        {
            Root       = root ?? throw new ArgumentNullException(nameof(root));
            Pool       = pool ?? throw new ArgumentNullException(nameof(pool));
            Queue      = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        public SolverNode     Root  { get; }
        public INodeLookup    Pool  { get; }
        public ISolverQueue   Queue { get; }
        public TreeStateCore? Alt   { get; set; }
        
        
        public virtual SolverNode? FindMatch(SolverNode find)
        {
            var match = Pool.FindMatch(find);
            if (match != null) return match;

            match = Queue.FindMatch(find);
            return match;
        }
    }
    
    public class TreeState : TreeStateCore
    {
        public TreeState(SolverNode root, INodeLookup pool, ISolverQueue queue, INodeEvaluator evaluator) : base(root, pool, queue)
        {
            Evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public TreeState(TreeStateCore core, INodeEvaluator eval) : base(core.Root, core.Pool, core.Queue)
        {
            Alt       = core.Alt;
            Evaluator = eval ?? throw new ArgumentNullException(nameof(eval));
        }
        
        
        public INodeEvaluator Evaluator { get; }
    }
    
    
    public abstract class SolverStateSingleTree : SolverState
    {
        protected SolverStateSingleTree(SolverCommand command, ISolver solver, TreeStateCore treeState) : base(command, solver)
        {
            TreeState = treeState;
        }

        public TreeStateCore TreeState { get; }
    }
    
    public abstract class SolverStateDoubleTree : SolverState
    {
        protected SolverStateDoubleTree(SolverCommand command, ISolver solver, TreeStateCore forward, TreeStateCore reverse) : base(command, solver)
        {
            Forward         = forward;
            Reverse         = reverse;
            SolutionsChains = new List<SolutionChain>();
        }
        
        public TreeStateCore       Forward         { get; }
        public TreeStateCore       Reverse         { get; }
        public List<SolutionChain> SolutionsChains { get;  }
    }

    public sealed class SolverStateEvaluationSingleThreaded : SolverStateSingleTree
    {
        public SolverStateEvaluationSingleThreaded(SolverCommand command, ISolver solver, TreeState treeState) : base(command, solver, treeState)
        {
        }

        public override IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors() => null;
    }
    
    
    public sealed class SolverStateForwardReverse : SolverStateDoubleTree
    {
        public SolverStateForwardReverse(SolverCommand command, SingleThreadedForwardReverseSolver solver, TreeState forward, TreeState reverse) : base(command, solver, forward, reverse)
        {
        }

        public new SingleThreadedForwardReverseSolver Solver => (SingleThreadedForwardReverseSolver)base.Solver;

        public override IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors() => null;
    }
    
    public sealed class SolverStateMultiThreaded : SolverStateDoubleTree
    {
        public SolverStateMultiThreaded(SolverCommand command, MultiThreadedForwardReverseSolver solver, TreeStateCore fwd, TreeStateCore rev) 
            : base(command, solver, fwd, rev)
        {
            IsRunning = false;
            Workers = new List<MultiThreadedForwardReverseSolver.SingleThreadWorker>();
        }

        public new MultiThreadedForwardReverseSolver Solver => (MultiThreadedForwardReverseSolver)base.Solver;
        
        public bool IsRunning  { get; set; }
        
        public List<MultiThreadedForwardReverseSolver.SingleThreadWorker> Workers    { get; }
        
        public override IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors()
        {
            yield return Solver;
        }
        
        public sealed class WorkerState : SolverStateDoubleTree
        {
            public WorkerState(SolverStateMultiThreaded parentState, TreeState primary) 
                : base(parentState.Command, parentState.Solver, parentState.Forward, parentState.Reverse)
            {
                ParentState = parentState ?? throw new ArgumentNullException(nameof(parentState));
                Primary     = primary ?? throw new ArgumentNullException(nameof(primary));
                StaticMaps  = parentState.StaticMaps;

                KnownSolutionTracker = parentState.KnownSolutionTracker;
            }

            public SolverStateMultiThreaded ParentState { get;  }
            public TreeState                Primary     { get; }

            public override IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors() => null;
        }
        
    }
    
}