using ConsoleZ.Win32;

namespace ConsoleZ.Drawing
{
    public class ConsoleRendererCHAR_INFO : ConsoleRenderer<CHAR_INFO>
    {
        public ConsoleRendererCHAR_INFO(IBufferedAbsConsole<CHAR_INFO> bufferedAbsConsole) : base(bufferedAbsConsole)
        {
        }

        public override void DrawText(int x, int y, string txt, CHAR_INFO style)
        {
            if (string.IsNullOrEmpty(txt)) return;
            
            var c = x;
            foreach (var chr in txt)
            {
                this[c++, y] = new CHAR_INFO(chr, style.Attributes);
            }
        }
    }
}