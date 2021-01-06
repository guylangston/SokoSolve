using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorInt;

namespace ConsoleZ.Drawing
{
    public interface IRenderer<TPixel>
    {
        int Width { get; }
        int Height { get; }

        RectInt Geometry { get; } // new RectInt(0, 0, Width, Height);

        void Fill(TPixel p);
        TPixel this[int x, int y] { get; set; }   // Get/Set Pixel
        TPixel this[VectorInt2 p] { get; set; }          // Get/Set Pixel
        TPixel this[float x, float y] { get; set; } // Get/Set Pixel
        // This should really include Vector2

        void DrawText(int x, int y, string txt, TPixel style);

        void Update();
    }
}
