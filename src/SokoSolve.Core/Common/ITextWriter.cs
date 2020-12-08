using System.IO;

namespace SokoSolve.Core.Common
{
    public interface ITextWriterBase
    {
        void WriteLine(string s);
    }
    
    public interface ITextWriter : ITextWriterBase
    {
        void Write(string s);
    }

    public static class TextWriterExt
    {
        public static void WriteLine(this ITextWriter tw) => tw.WriteLine(string.Empty);

        public static void WriteLine(this ITextWriter tw, string format, params object[] args)
            => tw.WriteLine(args == null
                ? format
                : string.Format(format, args));
    }

    public class TextWriterAdapter : ITextWriter
    {
        private readonly TextWriter tw;
        
        public static ITextWriter Console => new TextWriterAdapter(System.Console.Out);
        

        public TextWriterAdapter(TextWriter tw)
        {
            this.tw = tw;
        }

        public void WriteLine(string s)
        {
            lock (this)
            {
                tw.WriteLine(s);    
            }
            
        }

        public void Write(string s)
        {
            lock (this)
            {
                tw.Write(s);    
            }
            
        }
    }
}