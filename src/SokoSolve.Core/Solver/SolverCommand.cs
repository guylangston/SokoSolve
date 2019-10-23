using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Debugger;
using SokoSolve.Core.Game;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
   


    public class SolverCommand
    {
        public SolverCommand()
        {
            Debug = NullDebugEventPublisher.Instance;
            CheckAbort = x => CancellationToken.IsCancellationRequested;
        }

        public SolverCommand(SolverCommand rhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            Puzzle = rhs.Puzzle;
            Report = rhs.Report;
            ExitConditions = rhs.ExitConditions;
            CheckAbort = rhs.CheckAbort;
            Progress = rhs.Progress;
            Debug = rhs.Debug;

            CheckAbort = x => CancellationToken.IsCancellationRequested;
        }

        public Puzzle Puzzle { get; set; }

        public TextWriter Report { get; set; }

        public IDebugEventPublisher Debug { get; set; }

        public ExitConditions ExitConditions { get; set; }

        public Func<SolverCommand, bool> CheckAbort { get; set; }
        public CancellationToken CancellationToken { get; set; } = new CancellationToken();
        public IProgressNotifier Progress { get; set; }

        public ISolver Parent { get; set; }
    }

    public class SolutionChain
    {
        public SolverNode ForwardNode { get; set; }
        public SolverNode ReverseNode { get; set; }

        public INodeEvaluator FoundUsing { get; set; }
    }

    public class SolverCommandResult
    {
        public SolverCommandResult()
        {
            Statistics = new SolverStatistics();
        }

        public SolverCommand Command { get; set; }
        public StaticMaps StaticMaps { get; set; }
        public Exception Exception { get; set; }

        public SolverStatistics Statistics { get; set; }
        public bool EarlyExit { get; set; }
        public string ExitDescription { get; set; }

        public List<SolverNode> Solutions { get; set; }

        public List<SolutionChain> SolutionsWithReverse { get; set; }

        public bool HasSolution =>
            Solutions != null && Solutions.Any() ||
            SolutionsWithReverse != null && SolutionsWithReverse.Any();

        public ExitConditions.Conditions Exit { get; set; }

        public void ThrowErrors()
        {
            if (Exception != null) throw new Exception("Solver Failed", Exception);
        }

        public void WritePRE(object obj)
        {
            Command.Report.WriteLine("<pre class='map'>{0}</pre>", obj);
        }


        public List<Path> GetSolutions()
        {
            return SolverHelper.GetSolutions(this);
        }
    }


}