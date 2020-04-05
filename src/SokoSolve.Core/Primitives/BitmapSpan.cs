using System;
using VectorInt;

namespace SokoSolve.Core.Primitives
{
    public ref struct BitmapSpan // IBitmap
    {
        
        private readonly Span<uint> map;

        public BitmapSpan(VectorInt2 size, Span<uint> map)
        {
            this.Size = size;
            this.map = map;
        }
        public readonly VectorInt2 Size;
        
        public bool this[VectorInt2 pos]  =>(map[pos.Y] & (1 << pos.X)) > 0;
        public bool this[int        pX, int pY] => (map[pY] & (1 << pX)) > 0;

        public void SetBitwiseOR(IBitmap a, IBitmap b)
        {
            if (a is Bitmap aa && b is Bitmap bb)
            {
                for (var ccy = 0; ccy < map.Length; ccy++)
                {
                    this.map[ccy] = aa[ccy] | bb[ccy];
                }

                return;
            }
            
            throw new NotImplementedException();
        }
    }
}