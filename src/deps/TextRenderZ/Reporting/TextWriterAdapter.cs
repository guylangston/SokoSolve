using System;
using System.IO;

namespace TextRenderZ.Reporting
{
    public class TextWriterAdapter : ITextWriterAdapter
    {
        private readonly TextWriter outp;

        public TextWriterAdapter(TextWriter outp) => this.outp = outp;

        public static ITextWriterAdapter Console { get; } = new TextWriterAdapter(System.Console.Out);
        public static ITextWriterAdapter Null { get; } = new TextWriterAdapter(TextWriter.Null);

        public void Write(string?     s) => outp.Write(s);
        public void WriteLine(string? s) => outp.WriteLine(s);
        public void WriteLine()          => outp.WriteLine();
    }
}
