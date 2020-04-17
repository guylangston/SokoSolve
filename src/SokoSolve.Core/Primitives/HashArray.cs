using System;
using System.Numerics;

namespace SokoSolve.Core.Primitives
{
    
    public interface IHashArray<T>
    {
        int GetHashCode(T[]             array);
        int GetHashCode(ReadOnlySpan<T> span);
    }
    public interface IHashArrayByte :IHashArray<byte> { }
    public interface IHashArrayInt :IHashArray<int> { }
    public interface IHashArrayUInt :IHashArray<uint> { }

    public class BitmapHashOld : IHashArrayUInt
    {
        public int GetHashCode(uint[] array) => GetHashCode(array.AsSpan());

        public int GetHashCode(ReadOnlySpan<uint> span)
        {
            unchecked
            {
                uint result = 0;
                for (var ccy = 0; ccy < span.Length; ccy++)
                    result += span[ccy] / (uint) 1.2345 * (uint)ccy;

                return (int) result;    
            }
        }
    }
    
    
    public class BitmapHashWeighted : IHashArrayUInt
    {
        readonly uint[] weights;

        public BitmapHashWeighted(uint[] weights)
        {
            this.weights = weights;
        }

        public int GetHashCode(uint[] array) => GetHashCode(array.AsSpan());

        public int GetHashCode(ReadOnlySpan<uint> span)
        {
            unchecked
            {
                uint result = 1;
                for (var y = 0; y < span.Length; y++)
                {
                    if (span[y] == 0) continue;
                    result = result ^ (span[y] * weights[y]);
                }
                return (int)result; 
            }
        }
    }


    public class HashArrayByte : IHashArrayByte
    {
        // https://stackoverflow.com/a/53316768/914043
        #if NET47
        public int GetHashCode(byte[] array) => new BigInteger(array).GetHashCode();
        public int GetHashCode(ReadOnlySpan<byte> span) => new BigInteger(span.ToArray()).GetHashCode();
        #else
        public int GetHashCode(byte[] array) => GetHashCode(array.AsSpan());
        public int GetHashCode(ReadOnlySpan<byte> span) => new BigInteger(span).GetHashCode();
        #endif
    }

}