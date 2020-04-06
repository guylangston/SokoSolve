using System;
using System.IO;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
    public class SolverStatistics
    {
        public SolverStatistics()
        {
            Started   = DateTime.Now;
            Completed = DateTime.MinValue;
        }

        public int TotalNodes     { get; set; } = -1;
        public bool IsSample { get; set; }

        public int TotalDead      { get; set; } = -1;
        public int DepthCompleted { get; set; } = -1;
        public int DepthMax       { get; set; } = -1;
        public int DepthCurrent   { get; set; } = -1;
        public int Duplicates     { get; set; } = -1;

        public DateTime Started       { get; set; }
        public DateTime Completed     { get; set; }
        public TimeSpan Elapased      => (Completed == DateTime.MinValue ? DateTime.Now : Completed) - Started;
        public double   DurationInSec => Elapased.TotalSeconds;

        // Control
        public string? Name { get; set; }
        public string? Text { get; set; }
        public string? Type { get; set; }

      
        public string ToString(bool verbose)
        {
            if (DurationInSec == 0d || TotalNodes == 0)
            {
                return $"{TotalNodes,12:#,##0}";
            }
            
            var b = $"{TotalNodes,12:#,##0} nodes at {TotalNodes / DurationInSec,8:#,##0}/s in {Elapased.Humanize()}";

            if (verbose)
            {
                if (TotalDead >= 0) b += $" D:{TotalDead:#,##0}:{TotalDead * 100 / TotalNodes:0}%";
                if (Duplicates >= 0) b += $" Dup:{Duplicates:#,##0}:{Duplicates * 100 / TotalNodes:0}%";
            }
            
            return Name == null
                ? b
                : $"{Name,-40} {b}";
        }

        public string ToStringShort() => ToString(false);

        public override string ToString() => ToString(false);
    }
}