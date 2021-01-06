using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ConsoleZ.DisplayComponents;

namespace ConsoleZ
{
    // TODO: MaxLines and off-screen rules.
    public abstract class ConsoleBase : IConsoleWithProps, IFormatProvider, ICustomFormatter
    {
        private readonly ConcurrentDictionary<string, string> props = new ConcurrentDictionary<string, string>();
        protected List<string> lines = new List<string>();
        protected int linesRemoved;
        protected int count;

        protected ConsoleBase(string handle, int width, int height)
        {
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));
            Width = width;
            Height = height;
            Version = 0;
            Renderer = new PlainConsoleRenderer();
            Formatter = this;
        }

        public IConsole Parent { get; set; }

        public ICustomFormatter Formatter { get; set; } 
        public IConsoleRenderer Renderer { get; set; }
        
        public string Handle { get; }
        public int Version { get; private set; }

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public int DisplayStart => lines.Count < Height ? 0 : lines.Count - Height;
        public int DisplayEnd => lines.Count;

        public virtual string Title { get; set; }

        public int WriteLine(string s)
        {
            Parent?.WriteLine(s);
            lock (this)
            {
                return AddLineCheckLineFeed(s);
            }
        }

        public int WriteFormatted(FormattableString formatted)
        {
            Parent?.WriteFormatted(formatted);
            lock (this)
            {
                return AddLineCheckLineFeed(formatted.ToString(this));
            }
        }

        public virtual void Clear()
        {
            lines.Clear();
        }

        public bool UpdateLine(int line, string txt)
        {
            if (line < linesRemoved)
            {
                return false;
            }
            Parent?.UpdateLine(line, txt);
            lock (this)
            {
                return EditLine(line, txt);
            }
        }

        public bool UpdateFormatted(int line, FormattableString formatted)
        {
            if (line < linesRemoved)
            {
                return false;
            }
            Parent?.UpdateFormatted(line, formatted);
            lock (this)
            {
                return EditLine(line, formatted.ToString(this));
            }
        }
        
        public void SetProp(string key, string val)
        {
            props[key.ToLowerInvariant()] = val;
        }

        public bool TryGetProp(string key, out string val)
        {
            return props.TryGetValue(key.ToLowerInvariant(), out val);
        }

        string ICustomFormatter.Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null) return null;

            if (arg is DateTime dt)
            {
                return dt.ToString("yyyy-MM-dd");
            }
            if (arg is TimeSpan sp)
            {
                return ProgressBar.Humanize(sp);
            }

            return arg.ToString();
        }

        public object GetFormat(Type formatType)
        {
            return this;
        }

        private int AddLineCheckLineFeed(string s)
        {
            if (s != null && s.IndexOf('\n') > 0)
            {
                int last = 0;
                using (var tr = new StringReader(s))
                {
                    string l = null;
                    while ((l = tr.ReadLine()) != null)
                    {
                        last = AddLineCheckWrap(l);
                    }
                }

                return last;
            }
            else
            {
                return AddLineCheckWrap(s);
            }
                

            
        }

        protected virtual int AddLineCheckWrap(string l)
        {
            if (l == null)
            {
                return AddLineInner("");
            }
            if (l.Length > Width)
            {
                int last = 0;
                while (l.Length > Width)
                {
                    var front = l.Substring(0, Width);
                    last = AddLineInner(front);
                    l = l.Remove(0, front.Length);
                }

                if (l.Length > 0)
                {
                    last = AddLineInner(l);
                }

                return last;
            }
            else
            {
                return AddLineInner(l);
            }
        }

        /// <summary>
        /// Add to the internal buffer. Track/update DisplayStart
        /// </summary>
        /// <param name="l"></param>
        protected int AddLineInner(string l)
        {
            var indexAbs = count++;
            lines.Add(l);
            // Check screen up
            if (lines.Count > Height)
            {
                lines.RemoveAt(0);
                linesRemoved++;
            }

            var indexRel = indexAbs - linesRemoved;
            LineChanged(indexAbs, indexRel, l,false);

            Version++;

            return indexAbs;
        }



        private bool EditLine(int indexAbs, string txt)
        {
            if (txt.IndexOf('\n') > 0) throw new NotImplementedException();

            var indexRel = indexAbs - linesRemoved;
            if (indexRel > 0 && lines.Count > indexRel)
            {
                lines[indexRel] = txt;
                Version++;
                LineChanged(indexAbs, indexRel, txt, true);
                return true;
            }

            return false;
        }

        public abstract void LineChanged(int indexAbs, int indexRel, string line, bool updated);

        public virtual void Dispose()
        {
            if (Parent is IDisposable d) d.Dispose();
        }
    }
}