using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Debugger;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{

    public enum DuplicateMode
    {
        Discard,
        AddAsChild,
        ReuseInPool
    }

    public class SolverCommand
    {
        public SolverCommand(Puzzle puzzle, ExitConditions exitConditions)
        {
            Puzzle             = puzzle;
            ExitConditions     = exitConditions;
            Debug              = NullDebugEventPublisher.Instance;
            CancellationSource = new CancellationTokenSource();
            CheckAbort         = x => CancellationSource.IsCancellationRequested;
        }

        public SolverCommand(SolverCommand copy, Puzzle puzzle, ExitConditions exitConditions)
        {
            if (copy == null) throw new ArgumentNullException(nameof(copy));
            Puzzle             = puzzle;
            Report             = copy.Report;
            ExitConditions     = exitConditions;
            CheckAbort         = copy.CheckAbort;
            Progress           = copy.Progress;
            Debug              = copy.Debug;
            Parent             = null;
            ServiceProvider    = copy.ServiceProvider;
            AggProgress        = copy.AggProgress;
            CancellationSource = copy.CancellationSource;
            CheckAbort         = copy.CheckAbort;
        }

        public Puzzle                     Puzzle             { get; }
        public ExitConditions             ExitConditions     { get; }
        public ITextWriterBase?           Report             { get; set; }
        public IDebugEventPublisher       Debug              { get; set; }
        public Func<SolverCommand, bool>? CheckAbort         { get; set; }
        public CancellationTokenSource    CancellationSource { get; set; }
        public IProgressNotifier?         Progress           { get; set; }
        public ISolver?                   Parent             { get; set; }
        public ISolverContainer?          ServiceProvider    { get; set; }
        public IProgressNotifier?         AggProgress        { get; set; }
        public DuplicateMode              DuplicateMode      { get; set; }
        public Func<SolverNode, bool>?    Inspector          { get; set; }
    }

    public class SolutionChain
    {
        public SolverNode?     ForwardNode { get; set; }
        public SolverNode?     ReverseNode { get; set; }
        public INodeEvaluator? FoundUsing  { get; set; }
    }

    public class SolverState
    {
        public SolverState(SolverCommand command)
        {
            Command = command;
            Statistics   = new SolverStatistics();
        }

        public SolverCommand               Command               { get; }
        public SolverStatistics            Statistics            { get; }
        public StaticAnalysisMaps          StaticMaps            { get; set; }
        public Exception?                  Exception             { get; set; }
        public bool                        EarlyExit             { get; set; }
        public string?                     ExitDescription       { get; set; }
        public List<SolverNode>?           SolutionsNodes        { get; set; }
        public List<SolutionChain>?        SolutionsNodesReverse { get; set; }
        public List<Path>?                 Solutions             { get; set; }
        public List<(Path, string error)>? SolutionsInvalid      { get; set; }
        public ExitConditions.Conditions   Exit                  { get; set; }
        public SolverResultSummary?        Summary               { get; set; }
        

        public bool HasSolution => 
            (SolutionsNodes != null && SolutionsNodes.Any()) || 
            (SolutionsNodesReverse != null && SolutionsNodesReverse.Any());

        
        public void ThrowErrors()
        {
            if (Exception != null) throw new Exception("Solver Failed", Exception);
        }

        public virtual SolverNode? GetRootForward() => null;
        public virtual SolverNode? GetRootReverse() => null;
    }
}