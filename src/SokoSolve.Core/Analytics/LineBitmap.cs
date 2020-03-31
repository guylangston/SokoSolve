using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Analytics
{
    public class LineBitmap : Bitmap
    {
        public LineBitmap(int aSizeX, int aSizeY) : base(aSizeX, aSizeY)
        {
        }

        public LineBitmap(VectorInt2 aSize) : base(aSize)
        {
        }

        public LineBitmap(IBitmap copy) : base(copy)
        {
        }

        public LineBitmap(Bitmap copy) : base(copy)
        {
        }

        public VectorInt2 Start { get; set; }
        public VectorInt2 End   { get; set; }

        public override string ToString() => $"{Start} => {End}\n{base.ToString()}";
    }
}