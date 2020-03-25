using System;
using System.Text;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
    public class ExitConditions
    {
        public enum Conditions
        {
            InProgress,
            
            Continue,

            TotalNodes,
            TotalDead,
            Time,
            Solution,
            NothingLeftToDo,
            Aborted
        }

        public ExitConditions()
        {
            TotalNodes = int.MaxValue;
            TotalDead = int.MaxValue;
            Duration = TimeSpan.FromMinutes(3);
            StopOnSolution = true;
        }

        public int      TotalNodes     { get; set; }
        public int      TotalDead      { get; set; }
        public TimeSpan Duration       { get; set; }
        public bool     StopOnSolution { get; set; }
        public bool     ExitRequested  { get; set; }
        
        public static ExitConditions OneMinute()  => new ExitConditions
        {
            Duration       = TimeSpan.FromSeconds(60)
        };

        public static ExitConditions Default3Min() => new ExitConditions
        {
            Duration       = TimeSpan.FromMinutes(3),
        };

        public static ExitConditions Default10Min() => new ExitConditions
        {
            Duration       = TimeSpan.FromMinutes(10),
        };

        public Conditions ShouldExit(SolverCommandResult res)
        {
            if (ExitRequested) return Conditions.Aborted;
            
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
            var t = new StringBuilder();
            if (TotalNodes != int.MaxValue)
            {
                if (t.Length > 0) t.Append("; ");
                t.Append($"TotalNodes: {TotalNodes:#,##00}");
            }
            if (TotalDead != int.MaxValue)
            {
                if (t.Length > 0) t.Append("; ");
                t.Append($"TotalDead: {TotalDead:#,##00}");
            }
            
            if (t.Length > 0) t.Append("; ");
            t.Append($"Duration: {Duration.Humanize()}");

            return t.ToString();
        }
    }
}