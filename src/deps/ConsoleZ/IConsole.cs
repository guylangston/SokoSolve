using System;

namespace ConsoleZ
{
    public interface IConsoleWriter
    {
        /// <remarks> We don't use Write, rather update the same line</remarks>
        /// <returns>Absolute line index</returns>
        int WriteLine(string s);        

        /// <summary>Keep the input params for letter interrogation/formatting</summary>
        /// <returns>Absolute line index</returns>
        int WriteFormatted(FormattableString formatted);
    }


    public interface IConsole : IConsoleWriter
    {
        string Handle { get; }
        int Version { get;  }

        int Width { get; }
        int Height { get; }
        int DisplayStart { get;  }
        int DisplayEnd { get; }

        void Clear();
        
        /// <returns>false - unable to update</returns>
        bool UpdateLine(int lineAbsIndex, string txt);      

        /// <returns>false - unable to update</returns>
        bool UpdateFormatted(int lineAbsIndex, FormattableString formatted);
    }

    public interface IConsoleStream : IConsoleWriter
    {
        void Flush();
    }

    public interface IConsoleWithProps : IConsole, IDisposable
    {
        string Title { get; set; }
        
        /// <param name="key">Case Insensitive</param>
        void SetProp(string key, string val);

        /// <param name="key">Case Insensitive</param>
        bool TryGetProp(string key, out string val);
    }

    public interface IConsoleLineRenderer
    {
        string RenderLine(IConsole cons, int indexAbs, string raw);
    }

    public interface IAbsConsole<T>
    {
        string Handle { get; }
        int Width { get; }
        int Height { get; }
        T this[int x, int y] { get; set; }

        void Fill(T fill);
    }

    public interface IBufferedAbsConsole<T> : IAbsConsole<T>
    {
        void Update();
    }
}
