using System;
using System.Diagnostics;

namespace SokoSolve.Core.Common
{
    public class CodeBlockTimer
    {
        public CodeBlockTimer(string name, TimeSpan elapsed)
        {
            Elapsed = elapsed;
            Name     = name;
        }

        public TimeSpan Elapsed { get;  }
        public string Name { get;  }

        public override string ToString()
        {
            return $"{Name} [{Elapsed.Humanize()}]";
        }

        public static CodeBlockTimer Run(string name, Action code)
        {
            var timer = new Stopwatch();
            timer.Start();
            code();
            timer.Stop();
            return new CodeBlockTimer(name, timer.Elapsed);
        }
        
        public static CodeBlockTimer RunThenReport(string name, Action code, Action<CodeBlockTimer> report)
        {
            var timer = new Stopwatch();
            timer.Start();
            code();
            timer.Stop();
            var res =new CodeBlockTimer(name, timer.Elapsed);
            report(res);
            return res;
        }
    }
}