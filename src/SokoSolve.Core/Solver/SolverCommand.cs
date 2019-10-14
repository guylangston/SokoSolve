using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Debugger;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    public interface IProgressNotifier
    {
        void Update(ISolver caller, SolverCommandResult state, SolverStatistics global);
    }


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

        public Puzzle.Puzzle Puzzle { get; set; }

        public TextWriter Report { get; set; }

        public IDebugEventPublisher Debug { get; set; }

        public ExitConditions ExitConditions { get; set; }

        public Func<SolverCommand, bool> CheckAbort { get; set; }
        public CancellationToken CancellationToken { get; set; } = new CancellationToken();
        public IProgressNotifier Progress { get; set; }

        public ISolver Parent { get; set; }
    }

    public class ExitConditions
    {
        public enum Conditions
        {
            Continue,

            TotalNodes,
            TotalDead,
            Time,
            Solution,
            NothingLeftToDo,
            Aborted
        }

        public static readonly ExitConditions OneMinute = new ExitConditions
        {
            Duration = TimeSpan.FromSeconds(60),
            StopOnSolution = true,
            TotalNodes = int.MaxValue,
            TotalDead = int.MaxValue
        };

        public static readonly ExitConditions Default3Min = new ExitConditions
        {
            Duration = TimeSpan.FromMinutes(3),
            TotalNodes = 1000000,
            StopOnSolution = true,
            TotalDead = 500000
        };

        public static readonly ExitConditions Default10Min = new ExitConditions
        {
            Duration = TimeSpan.FromMinutes(10),
            TotalNodes = int.MaxValue,
            StopOnSolution = true,
            TotalDead = int.MaxValue
        };

        public int TotalNodes { get; set; }
        public int TotalDead { get; set; }

        public TimeSpan Duration { get; set; }

        public bool StopOnSolution { get; set; }

        public Conditions ShouldExit(SolverCommandResult res)
        {
            if (StopOnSolution && res.HasSolution) return Conditions.Solution;
            if (res.Statistics != null)
            {
                if (res.Statistics.TotalNodes >= TotalNodes) return Conditions.TotalNodes;
                if (DateTime.Now - res.Statistics.Started >= Duration)
                    return Conditions.Time; // TODO: This is unnessesarily slow
            }

            return Conditions.Continue;
        }

        public override string ToString()
        {
            return string.Format("TotalNodes: {0:#,##00}, TotalDead: {1}, Duration: {2}, StopOnSolution: {3}",
                TotalNodes, TotalDead, Duration, StopOnSolution);
        }
    }

    public class SolverStatistics
    {
        public SolverStatistics()
        {
            Started = DateTime.Now;
            Completed = DateTime.MinValue;
        }

        public int TotalNodes { get; set; }
        public int TotalDead { get; set; }
        public int DepthCompleted { get; set; }
        public int DepthMax { get; set; }
        public int DepthCurrent { get; set; }
        public int Duplicates { get; set; }

        public DateTime Started { get; set; }
        public DateTime Completed { get; set; }
        public TimeSpan Elapased => (Completed == DateTime.MinValue ? DateTime.Now : Completed) - Started;
        public double DurationInSec => Elapased.TotalSeconds;

        // Control
        public string Name { get; set; }
        public string Text { get; set; }


        public override string ToString()
        {
            return string.Format("{0}: {1:#,##0} nodes at {2:0.0} nodes/sec, Duration: {3}.",
                Name, TotalNodes, TotalNodes / DurationInSec, Elapased.ToString("d\\.hh\\:mm\\:ss"));
            //return string.Format("{4}: Nodes: {0:#,##0}, Dead: {1}, Duration: {2:0.0} sec., Duplicates: {3}", TotalNodes, TotalDead, DurationInSec, Duplicates, Name);
        }
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


    public interface ISolverQueue
    {
        SolverStatistics Statistics { get; }

        void Enqueue(SolverNode node);
        void Enqueue(IEnumerable<SolverNode> nodes);

        SolverNode Dequeue();
        SolverNode[] Dequeue(int count);
    }

    public interface ISolverNodeLookup
    {
        SolverStatistics Statistics { get; }

        void Add(SolverNode node);
        void Add(IEnumerable<SolverNode> nodes);

        SolverNode FindMatch(SolverNode node);
    }
}