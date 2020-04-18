using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core
{
    public static class VectorIntHelper
    {
        
        public static int ToLinearSpace(this VectorInt2 p, IBitmap bmp)
            => p.Y * bmp.Width + p.X;
        
        public static int ToLinearSpace(this VectorInt2 p, int width)
            => p.Y * width + p.X;
        
        public static int ToLinearSpace(this IBitmap map, VectorInt2 p)
            => p.Y * map.Width + p.X;

    }
}