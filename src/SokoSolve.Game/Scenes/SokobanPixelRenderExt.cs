using ConsoleZ.Drawing;
using ConsoleZ.Win32;
using VectorInt;

namespace SokoSolve.Game.Scenes
{
    public static class SokobanPixelRenderExt
    {
        public static void Box(this IRenderer<SokobanPixel> rr, IRectInt rect, SokobanPixel[]? pixel = null) 
            => RendererExt.Box<SokobanPixel>(rr, rect, pixel ?? AsciiBox);

        public static void TitleBox(this IRenderer<SokobanPixel> rr, IRectInt rect, string text, SokobanPixel[]? pixel = null)
        {
            pixel ??= AsciiBox;
            Box(rr, rect, pixel);
            rr.DrawText(rect.TM, $"[ {text} ]", pixel[0], TextAlign.Middle);
        }

        public static SokobanPixel[] AsciiBox = new[]
        {
            
            new SokobanPixel((char)0xda), // TL  0
            new SokobanPixel((char)0xc4), // TM  1
            new SokobanPixel((char)0xbf), // TR  2
                             
            new SokobanPixel((char)0xb3), // ML  3
            new SokobanPixel((char)' '),  // C   4
            new SokobanPixel((char)0xb3), // MR  5
                             
            new SokobanPixel((char)0xc0), // BL  6
            new SokobanPixel((char)0xc4), // BM  7
            new SokobanPixel((char)0xd9), // BR  8


        };


    }
}