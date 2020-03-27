using System;

namespace SokoSolve.Core
{
    public class Statistics
    {
        public Statistics()
        {
            Started = Completed = DateTime.MinValue;
        }

        // Standard
        public int Steps { get; set; }
        public int Pushes { get; set; }
        public int Undos { get; set; }
        public int Restarts { get; set; }
        public DateTime Started { get; set; }
        public DateTime Completed { get; set; }
        public TimeSpan Elapased => (Completed == DateTime.MinValue ? DateTime.Now : Completed) - Started;
        public double DurationInSec => Elapased.TotalSeconds;


        public override string ToString() => $"Steps: {Steps}, Pushes: {Pushes}, Undos: {Undos}, Restarts: {Restarts}";
    }
}