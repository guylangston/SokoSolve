using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleZ
{
    public class StdConsole : IConsole
    {
        private StdConsole() 
        {
        }
        private static readonly object locker = new object();
        private static volatile StdConsole singleton = null;

        public static StdConsole Singleton
        {
            get
            {
                if (singleton != null) return singleton;
                lock (locker)
                {
                    if (singleton == null)
                    {
                        var t = new StdConsole();
                        singleton = t;
                    }

                    return singleton;
                }
            }
        }

        public int WriteLine(string s)
        {
            System.Console.WriteLine(s);
            return -1;
        }

        public int WriteFormatted(FormattableString formatted)
        {
            System.Console.WriteLine(formatted);
            return -1;
        }

        public string Handle => "StdConsole";
        public int Version => 0;
        public int Width => Console.WindowWidth;
        public int Height => Console.WindowHeight;
        public int DisplayStart => 0;
        public int DisplayEnd => Console.WindowHeight;

        public void Clear()
        {
            System.Console.Clear();
        }

        public bool UpdateLine(int line, string txt)
        {
            return false;
        }

        public bool UpdateFormatted(int line, FormattableString formatted)
        {
            return false;
        }
    }
}
