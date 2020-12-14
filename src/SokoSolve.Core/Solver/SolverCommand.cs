using System;
using System.Threading;
using SokoSolve.Core.Common;
using SokoSolve.Core.Debugger;
using SokoSolve.Core.Lib;
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
        }

        public SolverCommand(SolverCommand copy, Puzzle puzzle, ExitConditions exitConditions)
        {
            if (copy == null) throw new ArgumentNullException(nameof(copy));
            Puzzle          = puzzle;
            Report          = copy.Report;
            ExitConditions  = exitConditions;
            Progress        = copy.Progress;
            Debug           = copy.Debug;
            Parent          = null;
            ServiceProvider = copy.ServiceProvider;
            AggProgress     = copy.AggProgress;

            CancellationSource = new CancellationTokenSource();
        }

        // Core
        public Puzzle             Puzzle          { get; }
        public PuzzleIdent        PuzzleIdent     { get; set; }  // Must be NON-NULL; generate a temp Ident if not stored in lib
        public ExitConditions     ExitConditions  { get; }
        public ISolverContainer   ServiceProvider { get; set; }
        public ITextWriterBase?   Report          { get; set; }
        public IProgressNotifier? Progress        { get; set; }
        
        // Mutlti
        public CancellationTokenSource CancellationSource { get; set; }
        public IProgressNotifier?      AggProgress        { get; set; }
        public ISolver?                Parent             { get; set; }
        
        // Debugging
        public DuplicateMode           DuplicateMode { get; set; }
        public IDebugEventPublisher?   Debug         { get; set; }
        public Func<SolverNode, bool>? Inspector     { get; set; }

        public bool CheckExit(SolverState? state, out ExitConditions.Conditions exit)
        {
            if (CancellationSource.IsCancellationRequested)
            {
                exit = ExitConditions.Conditions.Aborted;
                return true;
            }

            if (state == null)  // batch check
            {
                exit = ExitConditions.Conditions.Continue;
                return false;
            }
            
            exit = ExitConditions.ShouldExit(state);
            return exit != ExitConditions.Conditions.Continue;
        }


    }

    public class SolutionChain
    {
        public SolverNode?     ForwardNode { get; set; }
        public SolverNode?     ReverseNode { get; set; }
        public INodeEvaluator? FoundUsing  { get; set; }
        public Path            Path        { get; set; }
    }

}