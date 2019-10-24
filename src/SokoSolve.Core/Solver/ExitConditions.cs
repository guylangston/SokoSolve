using System;

namespace SokoSolve.Core.Solver
{
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

        public int      TotalNodes     { get; set; }
        public int      TotalDead      { get; set; }
        public TimeSpan Duration       { get; set; }
        public bool     StopOnSolution { get; set; }
        public bool     ExitRequested  { get; set; }
        
        public static readonly ExitConditions OneMinute = new ExitConditions
        {
            Duration       = TimeSpan.FromSeconds(60),
            StopOnSolution = true,
            TotalNodes     = int.MaxValue,
            TotalDead      = int.MaxValue
        };

        public static readonly ExitConditions Default3Min = new ExitConditions
        {
            Duration       = TimeSpan.FromMinutes(3),
            TotalNodes     = 1000000,
            StopOnSolution = true,
            TotalDead      = 500000
        };

        public static readonly ExitConditions Default10Min = new ExitConditions
        {
            Duration       = TimeSpan.FromMinutes(10),
            TotalNodes     = int.MaxValue,
            StopOnSolution = true,
            TotalDead      = int.MaxValue
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
            => $"TotalNodes: {TotalNodes:#,##00}, TotalDead: {TotalDead}, Duration: {Duration}, StopOnSolution: {StopOnSolution}";
    }
}