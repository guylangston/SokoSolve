using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Analytics;

namespace SokoSolve.Core.Solver
{
    public class SolverState
    {
        public SolverState(SolverCommand command, ISolver solver)
        {
            Command    = command;
            Solver     = solver;
            Statistics = new SolverStatistics();
        }

        public SolverCommand             Command         { get; }
        public ISolver                   Solver          { get; }
        public SolverStatistics          Statistics      { get; }
        public StaticAnalysisMaps        StaticMaps      { get; set; }
        public Exception?                Exception       { get; set; }
        public bool                      EarlyExit       { get; set; }
        public string?                   ExitDescription { get; set; }
        public List<SolverNode>?         SolutionsNodes  { get; set; }
        public List<SolutionChain>?      SolutionsChains { get; set; }
        public List<Path>?               Solutions       { get; set; }
        public ExitConditions.Conditions Exit            { get; set; }
        public SolverResultSummary?      Summary         { get; set; }
        

        public bool HasSolution => 
            (SolutionsNodes != null && SolutionsNodes.Any()) || 
            (SolutionsChains != null && SolutionsChains.Any());

        
        public void ThrowErrors()
        {
            if (Exception != null) throw new Exception("Solver Failed", Exception);
        }

        public virtual SolverNode? GetRootForward() => null;
        public virtual SolverNode? GetRootReverse() => null;
        
    }
    
    // TODO: Remove/Refactor away to use SolverTreeState instead
    public class SolverBaseState : SolverState
    {
        public SolverBaseState(SolverCommand command, ISolver solver) : base(command, solver)
        {
        }
        
        public SolverNode?     Root       { get; set; }
        public ISolverQueue?   Queue      { get; set; }
        public INodeLookup?    Pool       { get; set; }
        public INodeEvaluator? Evaluator  { get; set; }
        public SolverNode?     PeekOnTick { get; set; }

        public override SolverNode? GetRootForward() => Root;
    }
    
    public class SolverTreeState
    {
        public SolverNode?     Root        { get; set; }
        public ISolverQueue?   Queue       { get; set; }
        public INodeEvaluator? Evaluator   { get; set; }
        public INodeLookup?    PoolForward { get; set; }
        public INodeLookup?    PoolReverse { get; set; }
    }
    
    public class SingleThreadedSolverState : SolverState
    {
        public SingleThreadedSolverState(SolverCommand command, ISolver solver) : base(command, solver)
        {
        }

        public SolverTreeState? Forward { get; set; }
        public SolverTreeState? Reverse { get; set; }

        public override SolverNode? GetRootForward() => Forward?.Root;
        public override SolverNode? GetRootReverse() => Reverse?.Root;
    }
    
    public class MultiThreadedSolverState : SolverBaseState
    {
        public MultiThreadedSolverState(SolverCommand command, MultiThreadedForwardReverseSolver master) : base(command, master)
        {
        }
        
        public MultiThreadedSolverState(SolverCommand command, SingleThreadedForwardSolver slave) : base(command, slave)
        {
            
        }
        public MultiThreadedSolverState(SolverCommand command, SingleThreadedReverseSolver slave) : base(command, slave)
        {
            
        }

        public List<MultiThreadedForwardReverseSolver.Worker>? Workers      { get; set; }
        public List<SolverStatistics>?                         StatsInner   { get; set; }
        public bool                                            IsRunning    { get; set; }
        public INodeLookup?                                    PoolReverse  { get; set; }
        public INodeLookup?                                    PoolForward  { get; set; }
        public ISolverQueue                                    QueueForward { get; set; }
        public ISolverQueue                                    QueueReverse { get; set; }
        public SolverNode                                      RootReverse  { get; set; }
        public MultiThreadedSolverState?                       ParentState  { get; set; }

        public bool IsMaster => ParentState == null;

        public override SolverNode? GetRootForward() => Root;
        public override SolverNode? GetRootReverse() => RootReverse;
    }
}