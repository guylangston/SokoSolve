using System.Runtime.CompilerServices;

namespace SokoSolve.Primitives;

public static class BitArrayHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBit(ReadOnlySpan<byte> buffer, int bitIdx)
    {
        var byteIdx = bitIdx / 8;
        var byteBit = bitIdx % 8;
#if DEBUG
        ArgumentOutOfRangeException.ThrowIfNegative(bitIdx);
        if (byteIdx >= buffer.Length) throw new ArgumentOutOfRangeException(nameof(bitIdx));
#endif
        return (buffer[byteIdx] & (1 << byteBit)) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(Span<byte> buffer, int bitIdx, bool value)
    {
        var byteIdx = bitIdx / 8;
        var byteBit = bitIdx % 8;
#if DEBUG
        ArgumentOutOfRangeException.ThrowIfNegative(bitIdx);
        if (byteIdx >= buffer.Length) throw new ArgumentOutOfRangeException(nameof(bitIdx));
#endif
        buffer[byteIdx] = value
            ? (byte)(buffer[byteIdx] |  (1 << byteBit))
            : (byte)(buffer[byteIdx] & ~(1 << byteBit));
    }

    public static uint CalcBytes(uint sizeBits)
    {
        return (sizeBits / 8u) + (sizeBits % 8u > 0 ? 1u : 0u);
    }

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
}

