using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ConsoleZ
{
    /// <summary>
    /// ANSI Terminal Escape Codes
    /// http://www.lihaoyi.com/post/BuildyourownCommandLinewithANSIescapecodes.html
    ///
    /// Enable in Win10
    /// https://www.jerriepelser.com/blog/using-ansi-color-codes-in-net-console-apps/
    ///
    /// Wikipedia: ANSI escape code
    /// https://en.wikipedia.org/wiki/ANSI_escape_code
    /// </summary>
    public sealed class AnsiConsole : ConsoleBase, IConsoleLineRenderer
    {
        private static readonly object locker = new object();
        private static volatile AnsiConsole singleton = null;
        private AnsiConsole() : base("AnsiConsole", Console.BufferWidth, Console.BufferHeight)
        {
            // Attempt to sync the current screen and our buffer

            // Start on a clean line
            if (Console.CursorLeft != 0) Console.WriteLine();
            if (Console.CursorTop != 0)
            {
                for (int cc = 0; cc < Console.CursorTop; cc++)
                {
                    lines.Add("");
                }

                count = Console.CursorTop;
            }
            
            Renderer = new AnsiConsoleRenderer();
            Plain = new PlainConsoleRenderer();
        }

        public PlainConsoleRenderer Plain { get; set; }

        public static AnsiConsole Singleton
        {
            get
            {
                if (singleton != null) return singleton;
                lock (locker)
                {
                    if (singleton == null)
                    {
                        var t = new AnsiConsole();
                        t.EnableANSI();
                       
                        singleton = t;
                    }

                    return singleton;
                }
            }
        }

        #if WINDOWS
        public override string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }
        #endif

        public bool UsePrefix { get; set; }

        public override void Clear()
        {
            base.Clear();
            Console.Clear();
        }

        public override void LineChanged(int indexAbs, int indexRel, string line, bool updated)
        {
            if (updated)
            {
                var x = Console.CursorTop;

                Console.SetCursorPosition(0, indexRel);

                var rline = RenderLine(this, indexAbs, line);
                var pline = Plain.RenderLine(this, indexAbs, line);
                if (pline.Length > Width)
                {
                    Console.Write(pline.Substring(0, Width));
                }
                else
                {
                    Console.Write(rline + StringRepeat(' ', Width - pline.Length));
                }
                Console.CursorTop = x;
                Console.CursorLeft = 0;
            }
            else
            {
                
                Console.WriteLine(RenderLine(this, indexAbs,  line));
            }
        }

        private static string StringRepeat(char c, int len)
        {
            var arr = new char[len];
            for (int x = 0; x < len; x++)
            {
                arr[x] = c;
            }

            return new string(arr);
        }

        // TODO: This format should be user-controller (func?)
        public string RenderLine(IConsole cons, int indexAbs, string s)
        {
            s = Renderer.RenderLine(cons, indexAbs, s);
            if (UsePrefix)
            {
                return $"{Escape(35)}{indexAbs,4} |{Escape(0)} {s}";
            }
            else
            {
                return s;
            }
        }

        protected override int AddLineCheckWrap(string l)
        {
            if (l != null && l.Length + 6 > Width)
            {
                var last = 0;
                while (l.Length + 6 > Width)
                {
                    var front = l.Substring(0, Width - 7 );
                    last = AddLineInner(front);
                    l = l.Remove(0, front.Length);
                }

                if (l.Length > 0)
                {
                    AddLineInner(l);
                }
                return last;
            }
            else
            {
                return AddLineInner(l);
            }
        }

        public static string Escape(int clr) => $"\u001b[{clr}m";
        public static string EscapeFore(Color c) => $"\u001b[38;2;{c.R};{c.G};{c.B}m"; // https://en.wikipedia.org/wiki/ANSI_escape_code#24-bit
        public static string EscapeBack(Color c) => $"\u001b[48;2;{c.R};{c.G};{c.B}m"; // https://en.wikipedia.org/wiki/ANSI_escape_code#24-bit

        public void EnableANSI()
        {
            if (!Environment.OSVersion.VersionString.Contains("Windows")) return;
            
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                throw new Exception("failed to get output console mode");
            }

            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
            if (!SetConsoleMode(iStdOut, outConsoleMode))
            {
                throw new Exception($"failed to set output console mode, error code: {GetLastError()}");
            }
        }

        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

       
    }
}