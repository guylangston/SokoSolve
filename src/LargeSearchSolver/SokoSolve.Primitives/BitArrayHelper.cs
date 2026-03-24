using System.Numerics;
using System.Runtime.CompilerServices;

namespace SokoSolve.Primitives;

public static class BitArrayHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBit(ReadOnlySpan<byte> buffer, int bitIdx)
    {
        int byteIndex = bitIdx >> 3;         // n / 8
        int bitIndex = bitIdx & 7;           // n % 8
#if DEBUG
        ArgumentOutOfRangeException.ThrowIfNegative(bitIdx);
        if (byteIndex >= buffer.Length) throw new ArgumentOutOfRangeException(nameof(bitIdx));
#endif
        return ((buffer[byteIndex] >> bitIndex) & 1) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(Span<byte> buffer, int bitIdx, bool value)
    {
        int byteIndex = bitIdx >> 3;         // n / 8
        int bitIndex = bitIdx & 7;           // n % 8
#if DEBUG
        ArgumentOutOfRangeException.ThrowIfNegative(bitIdx);
        if (byteIndex >= buffer.Length) throw new ArgumentOutOfRangeException(nameof(bitIdx));
#endif
    if (value)
        buffer[byteIndex] |= (byte)(1 << bitIndex);
    else
        buffer[byteIndex] &= (byte)~(1 << bitIndex);
    }

    public static int CalcBytes(int sizeBits)
    {
        return (sizeBits / 8) + (sizeBits % 8 > 0 ? 1 : 0);
    }
    public static uint CalcBytes(uint sizeBits)
    {
        return (sizeBits / 8u) + (sizeBits % 8u > 0 ? 1u : 0u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBit(ReadOnlySpan<byte> buffer, int width, int height, int x, int y)
    {
#if DEBUG
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, width);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, height);
#endif
        return GetBit(buffer, (y * width) + x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(Span<byte> buffer, int width, int height, int x, int y, bool value)
    {
#if DEBUG
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, width);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, height);
#endif
        SetBit(buffer, (y * width) + x, value);
    }

    public static int Count(byte[] buffer)
    {
        // Use intrinics to count all bit=1
        int count = 0;
        for (int i = 0; i < buffer.Length; i++)
        {
            count += BitOperations.PopCount(buffer[i]);
        }
        return count;
    }
}

