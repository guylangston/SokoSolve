using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Game
{
    public class ConsoleElement : GameElement
    {
        public List<Line> lines = new List<Line>();
        private int cc;

        public IEnumerable<Line> Top(int count = 10)
        {
            if (lines.Count > count) return lines.Skip(lines.Count - count);
            return lines;
        }

        public void WriteLine(string format, params object[] args)
        {
            lines.Add(new Line
            {
                TimeToLive =  3,
                Text = string.Format(format, args)
            });
        }


        public override void Step(float elapsedSec)
        {
            foreach (var line in lines)
            {
                line.TimeToLive -= elapsedSec;
            }

            lines.RemoveAll(x => x.TimeToLive <= 0);
            
            base.Step(elapsedSec);
        }

        public class Line
        {
            public float TimeToLive { get; set; }
            public string Text { get; set; }

            public VectorInt2? Position { get; set; }
        }
    }
}