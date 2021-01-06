using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleZ
{
    public class BufferedFileConsole : ConsoleBase, IConsoleStream
    {
        private readonly TextWriter outp;

        public BufferedFileConsole(TextWriter outp,  string handle, int width, int height) : base(handle, width, height)
        {
            this.outp = outp ?? throw new ArgumentNullException(nameof(outp));
        }

        public override void LineChanged(int i, int index, string line, bool updated)
        {
            // Nothing, wait to the end

            // TODO: Drop TextWriter and use direct file path, with allows rewrites
            // TODO: Every 5min, write everything?
        }


        public override void Dispose()
        {
            base.Dispose();

            if (outp is StreamWriter sw && sw.BaseStream != null && !sw.BaseStream.CanWrite)
            {
                sw.Dispose();
                return;
            }

            try
            {
                if (Renderer == null)
                {
                    foreach (var line in base.lines)
                    {
                        outp.WriteLine(line);
                    }
                }
                else
                {
                    if (Renderer is IConsoleDocRenderer doc)
                    {
                        outp.WriteLine(doc.RenderHeader());
                        int cc = 0;
                        foreach (var line in base.lines)
                        {
                            outp.WriteLine(Renderer.RenderLine(this, cc++, line));
                        }
                        outp.WriteLine(doc.RenderFooter());
                    }
                    else
                    {
                        int cc = 0;
                        foreach (var line in base.lines)
                        {
                            outp.WriteLine(Renderer.RenderLine(this, cc++, line));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                // Don't throw Write
            }
            
            outp.Dispose();
        }

        public void Flush()
        {
            outp.Flush();
        }
    }
}
