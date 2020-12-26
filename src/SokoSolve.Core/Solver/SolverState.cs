using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Analytics;

namespace SokoSolve.Core.Solver
{
    public abstract class SolverState
    {
        protected SolverState(SolverCommand command)
        {
            Command     = command ?? throw new ArgumentNullException(nameof(command));
            GlobalStats = new SolverStatistics();
            Statistics  = new List<SolverStatistics>();
        }

        public SolverCommand          Command         { get; }
        
        public SolverStatistics       GlobalStats     { get; }
        public List<SolverStatistics> Statistics      { get; }
        public StaticAnalysisMaps     StaticMaps      { get; set; }
        public Exception?             Exception       { get; set; }
        public bool                   EarlyExit       { get; set; }
        public List<SolverNode>?      SolutionsNodes  { get; set; }
        public List<SolutionChain>?   SolutionsChains { get; set; }
        public List<Path>?            Solutions       { get; set; }
        public ExitResult             Exit            { get; set; }
        public SolverResultSummary?   Summary         { get; set; }
        
        public bool HasSolution => 
            (SolutionsNodes != null && SolutionsNodes.Any()) || 
            (SolutionsChains != null && SolutionsChains.Any());

        public void ThrowErrors()
        {
            if (Exception != null) throw new Exception("Solver Failed", Exception);
        }

        public abstract IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors();
    }
    
    
    public abstract class SolverStateEvaluation : SolverState
    {
        protected SolverStateEvaluation(SolverCommand command, ISolver solver, 
            INodeEvaluator evaluator, SolverNode root, INodeLookup pool, ISolverQueue queue, 
            INodeLookup? poolAlt, ISolverQueue? queueAlt) : base(command)
        {
            Solver    = solver ?? throw new ArgumentNullException(nameof(solver));
            Evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            Root      = root ?? throw new ArgumentNullException(nameof(root));
            Pool      = pool ?? throw new ArgumentNullException(nameof(pool));
            Queue     = queue ?? throw new ArgumentNullException(nameof(queue));
            PoolAlt   = poolAlt;
            QueueAlt  = queueAlt;
        }

        // Primary
        public ISolver        Solver    { get; }
        public INodeEvaluator Evaluator { get; }
        public SolverNode     Root      { get; }
        public INodeLookup    Pool      { get; }
        public ISolverQueue   Queue     { get; }
        
        // If Fwd<->Rev
        public INodeLookup?    PoolAlt    { get; }
        public ISolverQueue?   QueueAlt   { get; }
        
    }

    public sealed class SolverStateEvaluationSingleThreaded : SolverStateEvaluation
    {
        public SolverStateEvaluationSingleThreaded(SolverCommand command, ISolver solver, 
            INodeEvaluator evaluator, SolverNode root, INodeLookup pool, ISolverQueue queue, 
            INodeLookup? poolAlt, ISolverQueue? queueAlt) : base(command, solver, evaluator, root, pool, queue, poolAlt, queueAlt)
        {
        }
    }
    
    
    public sealed class SolverStateForwardReverse : SolverState
    {
        public SolverStateForwardReverse(SolverCommand command, SingleThreadedForwardReverseSolver solver) : base(command)
        {
            this.Solver = solver ?? throw new ArgumentNullException(nameof(solver));
        }

        public SingleThreadedForwardReverseSolver Solver  { get; }
        public SolverStateEvaluation              Forward { get;  }
        public SolverStateEvaluation              Reverse { get;  }
    }
    
    public sealed class SolverStateMultiThreaded : SolverState
    {
        public sealed class WorkerState : SolverStateEvaluation
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
        }
        
        public SolverStateMultiThreaded(SolverCommand command, ISolver solver) : base(command, solver)
        {
        }


        public INodeLookup? PoolReverse  { get; set; }
        public INodeLookup? PoolForward  { get; set; }
        public ISolverQueue QueueForward { get; set; }
        public ISolverQueue QueueReverse { get; set; }
        public SolverNode   RootReverse  { get; set; }
        
        public List<MultiThreadedForwardReverseSolver.Worker>? Workers    { get; set; }
        public List<SolverStatistics>                          StatsInner { get; set; }
        public bool                                            IsRunning  { get; set; }

        public override IEnumerable<IExtendedFunctionalityDescriptor> GetTypeDescriptors()
        {
            yield return Solver;
            
            yield return  PoolForward;
            yield return QueueForward;
            yield return  PoolForward;
            
            yield return  PoolReverse;
            yield return QueueReverse;
            yield return  PoolReverse;

            // foreach (var worker in Workers)
            // {
            //     yield return worker.Evaluator;
            // }
            
        }
        
    }
    
}