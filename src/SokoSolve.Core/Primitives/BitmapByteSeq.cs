using System;
using System.Collections;
using System.Collections.Generic;
using VectorInt;

namespace SokoSolve.Core.Primitives
{
    public class BitmapByteSeq : IBitmap
    {
        private Memory<byte> memory;

        public IEnumerable<bool> ForEachValue()
        {
            throw new NotImplementedException();
        }

        public int Width { get; }
        public int Height { get; }
        public VectorInt2 Size { get; }

        public bool this[int x, int y]
        {
            get
            {
                var offset = y * Width + x;
                var index = offset / 8;
                var bit = offset % 8;
                throw new NotImplementedException();
                //return (memory[index] & (1 << bit)) > 0;
            }
            set => throw new NotImplementedException();
        }

        public bool this[VectorInt2 p]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IEnumerator<(VectorInt2, bool)> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}