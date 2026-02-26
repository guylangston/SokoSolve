using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Schema;
using SokoSolve.Core.Common;
using SokoSolve.Core.Debugger;
using SokoSolve.Core.Lib;
using TextRenderZ.Reporting;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    public enum DuplicateMode
    {
        Discard,
        AddAsChild,
        ReuseInPool
    }

    public enum SafeMode
    {
        Off,
        Warning,    // Also adds additional locking
        Throw       // Also adds additional locking
    }

    public class SolverCommand
    {
        public SolverCommand(Puzzle puzzle, PuzzleIdent puzzleIdent, ExitConditions exitConditions, ISolverContainer serviceProvider,
            CancellationTokenSource? cancellationSource, ITextWriterAdapter? report, IProgressNotifier? progress, IProgressNotifier? aggProgress,
            ISolver? parent, DuplicateMode duplicateMode, IDebugEventPublisher? debug, Func<SolverNode, bool>? inspector)
        {
            // Required
            Puzzle             = puzzle ?? throw new ArgumentNullException(nameof(puzzle));
            ExitConditions     = exitConditions ?? throw new ArgumentNullException(nameof(exitConditions));
            ServiceProvider    = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            PuzzleIdent        = puzzleIdent ?? throw new ArgumentNullException(nameof(puzzleIdent));

            // Defaults
            CancellationSource = cancellationSource ?? new CancellationTokenSource();
            Debug              = debug ?? NullDebugEventPublisher.Instance;

            // Optional
            Report             = report;
            Progress           = progress;
            AggProgress        = aggProgress;
            Parent             = parent;
            DuplicateMode      = duplicateMode;
            Inspector          = inspector;

            Topology = new StrategyTopology();
            Topology.UsePoolEvalUnEval();
            //Topology.UsePoolEval_QueueUnEval();
        }

        public SolverCommand(LibraryPuzzle puzzle,  ExitConditions exitConditions, ISolverContainer serviceProvider) :
            this(puzzle.Puzzle, puzzle.Ident,
                exitConditions, serviceProvider, null, null, null, null,
                null, DuplicateMode.Discard, null, null)
        {

        }

        public SolverCommand(Puzzle puzzle, PuzzleIdent puzzleIdent, ExitConditions exitConditions, ISolverContainer serviceProvider) :
            this(puzzle, puzzleIdent,
                exitConditions, serviceProvider, null, null, null, null,
                null, DuplicateMode.Discard, null, null)
        {

        }

        // Core: required
        public Puzzle                  Puzzle             { get; }
        public ExitConditions          ExitConditions     { get; }
        public ISolverContainer        ServiceProvider    { get; }
        public CancellationTokenSource CancellationSource { get; }
        public PuzzleIdent             PuzzleIdent        { get; }  // Must be NON-NULL; generate a temp Ident if not stored in lib

        // Core: optional
        public SimpleArgs?         GeneralArgs { get; set; }
        public ITextWriterAdapter? Report      { get; set; }
        public IProgressNotifier?  Progress    { get; set; }

        // Algorithm flags
        public SafeMode         SafeMode                        { get; set; }
        public StrategyTopology Topology                        { get; set; }
        public bool             ConfirmDequeNotAlreadyProcessed { get; set; } = true;

        // Mutlti
        public IProgressNotifier? AggProgress { get; set; }
        public ISolver?           Parent      { get; set; }

        // Debugging
        public DuplicateMode           DuplicateMode { get; set; }
        public IDebugEventPublisher?   Debug         { get; set; }
        public Func<SolverNode, bool>? Inspector     { get; set; }

        public bool CheckExit(SolverState? state, out ExitResult exit)
        {
            if (CancellationSource.IsCancellationRequested)
            {
                exit = ExitResult.Aborted;
                return true;
            }

            if (state == null)  // batch check
            {
                exit = ExitResult.Continue;
                return false;
            }

            exit = ExitConditions.ShouldExit(state);
            return exit != ExitResult.Continue;
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
