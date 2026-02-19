using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SokoSolve.LargeSearchSolver;

public unsafe struct NodeStruct
{
    uint nodeid;
    uint parentid;
    uint siblingPrevId; // avoid array of children
    uint siblingNextId;
    int hashCode;
    byte type;      // 0 - fwd, 1 - rev
    byte playerX;
    byte playerY;
    byte playerPush;
    byte mapWidth;
    byte mapHeight;
    fixed uint mapCrate[32];   // bitmap 32x32
    fixed uint mapMove[32];    // bitmap 32x32

    public readonly uint NodeId => nodeid;
    public readonly uint ParentId => parentid;
    public readonly uint SiblingPrevId => siblingPrevId;
    public readonly uint SiblingNextId => siblingNextId;
    public readonly int  HashCode => hashCode;
    public readonly byte Type => type;
    public readonly byte PlayerX => playerX;
    public readonly byte PlayerY => playerY;
    public readonly byte PlayerPush => playerPush;

    public void SetParent(uint id) => parentid = id;
    public void SetNodeId(uint id) => nodeid = id;
    public void SetSiblingPrevId(uint id) => siblingPrevId = id;
    public void SetSiblingNextId(uint id) => siblingNextId = id;
    public void SetHashCode(int code) => hashCode = code;
    public void SetType(byte t) => type = t;
    public void SetPlayerX(byte x) => playerX = x;
    public void SetPlayerY(byte y) => playerY = y;
    public void SetPlayerPush(byte p) => playerPush = p;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetCrateMapAt(byte x, byte y)
    {
        Debug.Assert(y < 32);
        Debug.Assert(x < 32);
        Debug.Assert(x < mapWidth);
        Debug.Assert(y < mapHeight);

        return (mapCrate[y] & ((int)1 << (int)x)) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetMoveMapAt(byte x, byte y)
    {
        Debug.Assert(y < 32);
        Debug.Assert(x < 32);

        return (mapCrate[y] & ((int)1 << (int)x)) > 0;
    }

    public override string ToString()
    {
        return $"nodeid={nodeid}, parentid={parentid}";
    }
}


