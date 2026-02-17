using System;
using System.Runtime.CompilerServices;
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

        public bool this[VectorInt2 aPoint]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this[aPoint.X, aPoint.Y];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this[aPoint.X, aPoint.Y] = value;
        }

        public bool this[int pX, int pY]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (map[pY] & (1 << pX)) > 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => map[pY] = value
                ? map[pY] | (uint) (1 << pX)
                : map[pY] & ~(uint) (1 << pX);
        }

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

            for (var cy = 0; cy < Size.Y; cy++)
            for (var cx = 0; cx < Size.X; cx++)
                this[cx, cy] = a[cx, cy] || b[cx, cy];

        }
    }
}
