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

        public int  TotalNodes { get; set; } = -1;
        
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
            var builder = new FluentStringBuilder()
                .IfNotNull(Name, x => $"{x,-40}");
            
            if (DurationInSec <= 0d || TotalNodes == 0)
            {
                return builder.Append($"{TotalNodes,12:#,##0} nodes");
            }

            builder.When(verbose, then => 
               then.If(TotalDead >= 0, () => $" D:{TotalDead:#,##0}:{TotalDead * 100 / TotalNodes:0}%")
                   .If(Duplicates >= 0, () => $" D:{Duplicates:#,##0}:{Duplicates * 100 / TotalNodes:0}%"))
            .Append($"{TotalNodes,12:#,##0} nodes at {TotalNodes / DurationInSec,8:#,##0}/s in {Elapased.Humanize()}");

            return builder;
        }

        public string ToStringShort() => ToString(false);

        public override string ToString() => ToString(false);
    }
}