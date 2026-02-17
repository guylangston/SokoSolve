using System.Runtime.CompilerServices;

namespace SokoSolve.Core.Primitives
{
    public static class BitIndexedByteArray
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(byte[] arr, int offset)
        {
            var index = offset / 8;
            var bit   = offset % 8;
            return (arr[index] & (1 << bit)) > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(byte[] arr, int offset, bool val)
        {
            var index = offset / 8;
            var bit   = (byte)(offset % 8);
            var t     = 1 << bit;
            arr[index] = val
                ? (byte)(arr[index] |  t)
                : (byte)( arr[index] & ~t);
        }
    }
}
