using System.IO;
using TextRenderZ.Reporting;

namespace SokoSolve.Core.Common
{
   
    public static class TextWriterExt
    {
        public static void WriteLine(this ITextWriterAdapter tw) => tw.WriteLine(string.Empty);
    
        public static void WriteLine(this ITextWriterAdapter tw, string format, params object[] args)
            => tw.WriteLine(args == null
                ? format
                : string.Format(format, args));
    }

   
}