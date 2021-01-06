using System;
using VectorInt;

namespace ConsoleZ.Drawing
{
    public abstract class RendererBridge<TA, TB> : IRenderer<TA>
    {
        private readonly IRenderer<TB> target;

        protected RendererBridge(IRenderer<TB> target)
        {
            this.target = target ?? throw new NullReferenceException(nameof(target));
        }

        public int     Height   => target.Height;
        public int     Width    => target.Width;
        public RectInt Geometry => target.Geometry;

        protected abstract TB Convert(TA a);
        protected abstract TA Convert(TB a);

        public void Fill(TA p) => target.Fill(Convert(p));

        public TA this[int x, int y]
        {
            get => Convert(target[x, y]);
            set => target[x, y] = Convert(value);
        }

        public TA this[VectorInt2 p]
        {
            get => Convert(target[p]);
            set => target[p] = Convert(value);
        }

        public TA this[float x, float y]
        {
            get => Convert(target[x, y]);
            set => target[x, y] = Convert(value);
        }

        public void DrawText(int x, int y, string txt, TA style) => target.DrawText(x, y, txt, Convert(style));

        public void Update() => target.Update();
    }
    
    
}