using System;
using System.Windows.Media;
using ConsoleZ.Drawing;
using SkiaSharp;
using VectorInt;

namespace SokoSolve.Client.WPF
{
    public abstract class SkiaSharpTileRenderer<T> : IRenderer<T> 
    {
        protected SKSurface surface;
        private readonly T[,] buffer;

        protected SkiaSharpTileRenderer(SKSurface surface, int cellWidth, int cellHeight)
        {
            this.surface = surface;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            Width  = (int)surface.Canvas.LocalClipBounds.Width / cellWidth;
            Height = (int)surface.Canvas.LocalClipBounds.Height / CellHeight;

            buffer = new T[Width, Height];
        }

        public int CellWidth  { get; }
        public int CellHeight { get; }
        public int Width      { get; }
        public int Height     { get; }
        public RectInt Geometry => new RectInt(0, 0, Width, Height);

        protected abstract void DrawTile(VectorInt2 p, T tile);
        protected abstract void DrawTile(VectorInt2 p, char chr, T style);
        public abstract void DrawText(int x, int y, string txt, T style);
        
        public void Fill(T tile)
        {
            foreach (var p in this.Geometry)
            {
                this[p] = tile;
            }
        }
        
        public T this[VectorInt2 p]
        {
            get => buffer[p.X, p.Y];
            set
            {
                if (Geometry.Contains(p))
                {
                    buffer[p.X, p.Y] = value;
                    DrawTile(p, value);
                }
                
                
            }
        }

        public T this[int x, int y]
        {
            get => this[new VectorInt2(x, y)];
            set => this[new VectorInt2(x, y)] = value;
        }

        public T this[float x, float y]
        {
            get => this[(int)x, (int)y];
            set => this[(int)x, (int)y] = value;
        }
        
        public void Update()
        {
            
        }
    }
}