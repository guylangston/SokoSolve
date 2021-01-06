using VectorInt;

namespace ConsoleZ.Drawing
{
    public  abstract class ConsoleRenderer<TPixel> : IRenderer<TPixel>
    {
        private readonly IBufferedAbsConsole<TPixel> console;

        protected ConsoleRenderer(IBufferedAbsConsole<TPixel> console)
        {
            this.console  = console;
            this.Geometry = new RectInt(0, 0, console.Width, console.Height);
        }

        public int     Height   => console.Height;
        public int     Width    => console.Width;
        public RectInt Geometry { get; }

        public string Handle => console.Handle;

        public void Fill(TPixel p) => console.Fill(p);

        public TPixel this[int x, int y]
        {
            get => console[x,y];
            set => console[x, y] = value;
        }

        public TPixel this[VectorInt2 p]
        {
            get => console[p.X, p.Y];
            set => console[p.X, p.Y] = value;
        }

        public TPixel this[float x, float y]
        {
            get => console[(int)x, (int)y];
            set => console[(int)x, (int)y] = value;
        }

        public abstract void DrawText(int x, int y, string txt, TPixel style);

        public void Update() => console.Update();
    }
}