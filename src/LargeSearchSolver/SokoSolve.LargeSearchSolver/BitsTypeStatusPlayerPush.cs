namespace SokoSolve.LargeSearchSolver;

// source: type_status_playerpush
// BITS for type, status, playerpush
// 0    type
// 1    status
// 2    status
// 3    status
// 4    status (allows 16 status items)
// 5    playerpush
// 6    playerpush
// 7    playerpush (allows 8 directions, we use 5)
public static class BitsTypeStatusPlayerPush
{
    // bit offsets
    private const int TypeShift = 0;
    private const int StatusShift = 1;     // bits 1-4
    private const int PushShift = 5;       // bits 5-7

    // masks (already shifted into position)
    private const byte TypeMask   = 0b0000_0001;
    private const byte StatusMask = 0b0001_1110;
    private const byte PushMask   = 0b1110_0000;

    // Pack (union) fields into one byte
    public static byte Pack(bool type, int status, int pushDir)
    {
        if ((uint)status > 15) throw new ArgumentOutOfRangeException(nameof(status), "Must be 0..15");
        if ((uint)pushDir > 7) throw new ArgumentOutOfRangeException(nameof(pushDir), "Must be 0..7");

        byte b = 0;
        b |= (byte)((type ? 1 : 0) << TypeShift);
        b |= (byte)(status << StatusShift);
        b |= (byte)(pushDir << PushShift);
        return b;
    }

    public static int GetType(byte b) => b & TypeMask;
    public static int GetStatus(byte b) => (b & StatusMask) >> StatusShift;
    public static int GetPushDir(byte b) => (b & PushMask) >> PushShift;

    public static byte SetType(byte b, byte type)
    {
        b = (byte)(b & ~TypeMask);                       // clear bit 0
        b |= (byte)(type << TypeShift);        // OR in new bit
        return b;
    }

    public static byte SetStatus(byte b, int status)
    {
        if ((uint)status > 15) throw new ArgumentOutOfRangeException(nameof(status), "Must be 0..15");
        b = (byte)(b & ~StatusMask);                    // clear bits 1-4
        b |= (byte)(status << StatusShift);            // OR in new value
        return b;
    }

    public static byte SetPushDir(byte b, int pushDir)
    {
        if ((uint)pushDir > 7) throw new ArgumentOutOfRangeException(nameof(pushDir), "Must be 0..7");
        b = (byte)(b & ~PushMask);                      // clear bits 5-7
        b |= (byte)(pushDir << PushShift);              // OR in new value
        return b;
    }
}



