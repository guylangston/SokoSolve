using System;
using System.IO;

namespace SokoSolve.Core.Debugger
{
    public class NamedDebugEvent : IDebugEvent
    {
        public NamedDebugEvent(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class ConsoleDebugEventPublisher : IDebugEventPublisher
    {
        public static readonly ConsoleDebugEventPublisher Instance = new ConsoleDebugEventPublisher();

        public void Raise(object source, IDebugEvent dEvent, object context = null)
        {
            Console.WriteLine("[{0}] Source: {1}, Context: {2}", dEvent, source, context);
        }

        public void RaiseFormat(object source, IDebugEvent dEvent, string stringFormat, params object[] args)
        {
            Console.WriteLine(stringFormat, args);
        }
    }

    public class TextWriterDebugEventPublisher : IDebugEventPublisher
    {
        private readonly TextWriter stream;

        public TextWriterDebugEventPublisher(TextWriter stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            this.stream = stream;
        }

        public void Raise(object source, IDebugEvent dEvent, object context = null)
        {
            stream.WriteLine("[{0}] Source: {1}, Context: {2}", dEvent, source, context);
        }

        public void RaiseFormat(object source, IDebugEvent dEvent, string stringFormat, params object[] args)
        {
            stream.WriteLine(stringFormat, args);
        }
    }

    public sealed class NullDebugEventPublisher : IDebugEventPublisher
    {
        public static readonly NullDebugEventPublisher Instance = new NullDebugEventPublisher();

        public void Raise(object source, IDebugEvent dEvent, object context = null)
        {
        }

        public void RaiseFormat(object source, IDebugEvent dEvent, string stringFormat, params object[] args)
        {
        }
    }
}