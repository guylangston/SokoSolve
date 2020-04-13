using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VectorInt;

namespace SokoSolve.Core.Primitives
{
    public class BitmapByteSeq : IBitmap
    {
        private byte[] memory;
        public int baseIndex;
        
        public BitmapByteSeq(VectorInt2 size)
        {
            this.memory    = new byte[size.X * size.Y / 8  + 1];
            this.baseIndex = 0;
            Size           = size;
        }


        public BitmapByteSeq(byte[] memory, int baseIndex, VectorInt2 size)
        {
            this.memory = memory;
            this.baseIndex = baseIndex;
            Size = size;
        }

        public IEnumerable<bool> ForEachValue()
        {
            throw new NotImplementedException();
        }

        public int Width => Size.X;
        public int Height => Size.Y;
        public VectorInt2 Size { get; }

        public bool this[int x, int y]
        {
            get
            {
                var offset = baseIndex + y * Width + x;
                var index = offset / 8;
                var bit = offset % 8;
                
                return (memory[index] & (1 << bit)) > 0;
            }
            set
            {
                var offset = baseIndex + y * Width + x;
                var index  = offset / 8;
                var bit    = (byte)(offset % 8);
                var t = 1 << bit;
                memory[index] = value 
                    ? (byte)(memory[index] |  t)
                    : (byte)( memory[index] & ~t);
            }
        }

        public bool this[VectorInt2 aPoint]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this[aPoint.X, aPoint.Y];
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this[aPoint.X, aPoint.Y] = value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<(VectorInt2, bool)> GetEnumerator()
        {
            for (var yy = 0; yy < Height; yy++)
                for (var xx = 0; xx < Width; xx++)
                yield return (new VectorInt2(xx, yy), this[xx, yy]);
        }

        
    }
}