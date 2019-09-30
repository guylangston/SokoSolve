using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Game
{
    public class ConsoleElement : GameElement
    {
        protected readonly List<Line> lines = new List<Line>();
        private int cc = 0;
        private const int removeSpeed = 200;

        public class Line
        {
            public DateTime TimeStamp { get; set; }
            public string Text { get; set; }

            public VectorInt2? Position { get; set; }
        }


        public IEnumerable<Line> Top(int count = 10)
        {
            if (lines.Count > count)
            {
                return lines.Skip(lines.Count - count);
            }
            return lines;
        }
        

        public void WriteLine(string format, params object[] args)
        {
            lines.Add(new Line()
            {
                TimeStamp = DateTime.Now,
                Text = string.Format(format, args)
            }); 
        }

      
        public override void Step()
        {
            cc++;
            if (cc % removeSpeed == 0)
            {
                if (lines.Count > 3)
                {
                    lines.RemoveAt(0);
                }
            }
            base.Step();
        }
    }
}