using System;
using System.Collections.Generic;
using System.Drawing;
using ConsoleZ.Drawing;
using SkiaSharp;
using Tetris.Lib;
using VectorInt;

namespace SokoSolve.Client.WPF
{
    public class SkiaConsolePixelRenderer : SkiaSharpTileRenderer<ConsolePixel>
    {
        public SkiaConsolePixelRenderer(SKSurface surface, int cellWidth, int cellHeight) : base(surface, cellWidth, cellHeight)
        {
        }

        protected override void DrawTile(VectorInt2 p, ConsolePixel tile)
        {
            surface.Canvas.DrawRect(p.X * CellWidth, p.Y * CellHeight, CellWidth, CellHeight, GetSKPaint(tile.Back));

            var forePaint = GetSKPaint(tile.Fore);
            var s = forePaint.MeasureText(tile.Char.ToString());
            surface.Canvas.DrawText(tile.Char.ToString(), p.X * CellWidth + s/2 , p.Y * CellHeight + CellHeight - 2, forePaint);
        }

        private Dictionary<Color, SKPaint> clr = new Dictionary<Color, SKPaint>(); 
            
        private SKPaint GetSKPaint(in Color tileBack)
        {
            return new SKPaint()
            {
                //Color    = new SKColor(tileBack.R, tileBack.G, tileBack.B, tileBack.A),
                Color = new SKColor(tileBack.R, tileBack.G, tileBack.B),
                TextSize = CellWidth,
            };
        }

        protected override void DrawTile(VectorInt2 p, char chr, ConsolePixel style)
        {
            this[p] = new ConsolePixel(chr, style.Fore, style.Back);
        }

        public override void DrawText(int x, int y, string txt, ConsolePixel style)
        {
            var p = new VectorInt2(x, y);
            foreach (var chr in txt)
            {
                this[p] = new ConsolePixel(chr, style.Fore, style.Back);
                p = p.AddX(1);
            }
        }
    }
}