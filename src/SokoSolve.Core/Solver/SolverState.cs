using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Analytics;

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
        public List<SolverNode>       SolutionsNodes  { get;  }
        public List<Path>             Solutions       { get;  }
        public StaticAnalysisMaps     StaticMaps      { get; set; }
        public Exception?             Exception       { get; set; }
        public bool                   EarlyExit       { get; set; }
        public ExitResult             Exit            { get; set; }
        public SolverResultSummary?   Summary         { get; set; }
        
        public virtual bool HasSolution =>  SolutionsNodes.Any();

        public void ThrowErrors()
        {
            if (Exception != null) throw new Exception("Solver Failed", Exception);
        }

        public abstract IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors();
    }


    public class TreeState
    {
        public TreeState(SolverNode root, INodeLookup pool, ISolverQueue queue)
        {
            Root = root;
            Pool = pool;
            Queue = queue;
        }

        public SolverNode Root { get; }
        public INodeLookup Pool { get; }
        public ISolverQueue Queue { get; }
        public TreeState? Alt { get; set; }
    }

    public class TreeStateWithEval : TreeState
    {
        public TreeStateWithEval(SolverNode root, INodeLookup pool, ISolverQueue queue, INodeEvaluator evaluator) : base(root, pool, queue)
        {
            Evaluator = evaluator;
        }

        public INodeEvaluator Evaluator { get; }
    }
    
    public abstract class SolverStateSingle : SolverState
    {
        // protected SolverStateSingle(SolverCommand command, ISolver solver, 
        //     INodeEvaluator evaluator, SolverNode root, INodeLookup pool, ISolverQueue queue, 
        //     INodeLookup? poolAlt, ISolverQueue? queueAlt) : base(command, solver)
        // {
        //     Evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        //     Root      = root ?? throw new ArgumentNullException(nameof(root));
        //     Pool      = pool ?? throw new ArgumentNullException(nameof(pool));
        //     Queue     = queue ?? throw new ArgumentNullException(nameof(queue));
        //     PoolAlt   = poolAlt;
        //     QueueAlt  = queueAlt;
        // }

        public List<SolutionChain>    SolutionsChains { get;  }

        private TreeStateWithEval TreeState { get; }
        // // Primary
        // public INodeEvaluator Evaluator { get; }
        // public SolverNode     Root      { get; }
        // public INodeLookup    Pool      { get; }
        // public ISolverQueue   Queue     { get; }
        //
        // // If Fwd<->Rev
        // public INodeLookup?    PoolAlt    { get; }
        // public ISolverQueue?   QueueAlt   { get; }
    }
    
    public abstract class SolverStateFwdRev : SolverState
    {
        public List<SolutionChain>    SolutionsChains { get;  }
        
        public TreeStateWithEval Forward { get; }
        public TreeStateWithEval Reverse { get; }
    }

    public sealed class SolverStateEvaluationSingleThreaded : SolverStateSingle
    {
        public SolverStateEvaluationSingleThreaded(SolverCommand command, ISolver solver, 
            INodeEvaluator evaluator, SolverNode root, INodeLookup pool, ISolverQueue queue, 
            INodeLookup? poolAlt, ISolverQueue? queueAlt) : base(command, solver, evaluator, root, pool, queue, poolAlt, queueAlt)
        {
        }

        public override IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors() => null;
    }
    
    
    public sealed class SolverStateForwardReverse : SolverState
    {
        public SolverStateForwardReverse(SolverCommand command, SingleThreadedForwardReverseSolver solver) : base(command, solver)
        {
        }

        public new SingleThreadedForwardReverseSolver Solver => (SingleThreadedForwardReverseSolver)base.Solver;
        public TreeState Forward { get;  }
        public TreeState Reverse { get;  }
        
        
        public override IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors() => null;
    }
    
    public sealed class SolverStateMultiThreaded : SolverState
    {
        public sealed class WorkerState : SolverStateSingle
        {
            public WorkerState(SolverStateMultiThreaded parentState, 
                SolverCommand command, ISolver solver, INodeEvaluator evaluator, 
                SolverNode root, INodeLookup pool, ISolverQueue queue, 
                INodeLookup? poolAlt, ISolverQueue? queueAlt) 
                : base(command, solver, evaluator, root, pool, queue, poolAlt, queueAlt)
            {
                ParentState = parentState;
            }

            public SolverStateMultiThreaded ParentState { get;  }    
            
            public override IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors() => null;
        }
        
        public SolverStateMultiThreaded(SolverCommand command, MultiThreadedForwardReverseSolver solver, TreeState fwd, TreeState rev) 
            : base(command, solver)
        {
            Forward = fwd;
            Reverse = rev;
            IsRunning = false;
            Workers = new List<MultiThreadedForwardReverseSolver.Worker>();
            StatsInner = new List<SolverStatistics>();
        }

        public MultiThreadedForwardReverseSolver Solver { get; }
        public TreeState Forward { get; }
        public TreeState Reverse { get; }

        public bool IsRunning  { get; set; }
        
        public List<MultiThreadedForwardReverseSolver.Worker> Workers    { get; }
        public List<SolverStatistics>                          StatsInner { get;  }

        public override IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors()
        {
            yield return Solver;
        }
        
    }
    
}