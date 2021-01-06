namespace ConsoleZ.Drawing
{
    public class ConsolePixelRenderer : ConsoleRenderer<ConsolePixel>
    {
        public ConsolePixelRenderer(IBufferedAbsConsole<ConsolePixel> console) : base(console)
        {
        }

        public override void DrawText(int x, int y, string txt, ConsolePixel style)
        {
            var c = x;
            foreach (var chr in txt)
            {
                this[c++, y] = new ConsolePixel(chr, style.Fore, style.Back);
            }
        }
    }
}