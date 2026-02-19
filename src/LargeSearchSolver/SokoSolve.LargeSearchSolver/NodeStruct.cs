using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.LargeSearchSolver;

public enum NodeStatus
{
    ALLOC,
    LEASED,
    EVAL_START,
    EVAL_END,
    NEW_CHILD,
    EVAL_ALL_CHILDREN,
    DUPLICATE,
    COMPELTE
}

public unsafe struct NodeStruct
{
    uint nodeid;
    uint parentid;
    uint firstChildId; // avoid array of children
    uint siblingNextId;
    int hashCode;
    byte type;      // 0 - fwd, 1 - rev
    byte status;    //
    byte playerX;
    byte playerY;
    sbyte playerPushX;
    sbyte playerPushY;
    byte mapWidth;      // width,height are not strictly needed, but stops having to pass in the sizes each time the map is read
    byte mapHeight;
    const int MaxMapHeight = 32;
    fixed uint mapCrate[MaxMapHeight];   // bitmap 32x32
    fixed uint mapMove[MaxMapHeight];    // bitmap 32x32

    public NodeStruct()
    {
        Reset();
    }

    public readonly uint NodeId => nodeid;
    public readonly uint ParentId => parentid;
    public readonly uint FirstChildId => firstChildId;
    public readonly uint SiblingNextId => siblingNextId;
    public readonly int  HashCode => hashCode;
    public readonly byte Type => type;
    public readonly NodeStatus Status => (NodeStatus)status;
    public readonly byte PlayerX => playerX;
    public readonly byte PlayerY => playerY;
    public readonly sbyte PlayerPushX => playerPushX;
    public readonly sbyte PlayerPushY => playerPushY;
    public readonly byte Width => mapWidth;
    public readonly byte Height => mapHeight;

    public uint GetMapLineCrate(int idx) => mapCrate[idx];
    public uint GetMapLineMove(int idx) => mapMove[idx];

    public void SetParent(uint id) => parentid = id;
    public void SetNodeId(uint id) => nodeid = id;
    public void SetFirstChildId(uint id) => firstChildId = id;
    public void SetSiblingNextId(uint id) => siblingNextId = id;
    public void SetHashCode(int code) => hashCode = code;
    public void SetType(byte t) => type = t;
    public void SetStatus(NodeStatus t) => status = (byte)t;
    public void SetPlayerX(byte x) => playerX = x;
    public void SetPlayerY(byte y) => playerY = y;
    public void SetPlayerPush(sbyte dX, sbyte dY)
    {
        playerPushX = dX;
        playerPushY = dY;
    }

    public void Reset()
    {
        nodeid = uint.MaxValue;
        parentid = uint.MaxValue;
        firstChildId = uint.MaxValue;
        siblingNextId = uint.MaxValue;
        hashCode = 0;
        type = 0;
        playerX = 0;
        playerY = 0;
        playerPushX = 0;
        playerPushY = 0;
        mapWidth = 0;
        mapHeight = 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetCrateMapAt(byte x, byte y)
    {
        Debug.Assert(y < MaxMapHeight);
        Debug.Assert(x < MaxMapHeight);
        Debug.Assert(x < mapWidth);
        Debug.Assert(y < mapHeight);

        return (mapCrate[y] & ((int)1 << (int)x)) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCrateMapAt(byte x, byte y, bool val)
    {
        Debug.Assert(y < MaxMapHeight);
        Debug.Assert(x < MaxMapHeight);
        Debug.Assert(x < mapWidth);
        Debug.Assert(y < mapHeight);

        fixed(uint* ptr = &mapCrate[0])
        {
            var span = new BitmapSpan(new VectorInt2(mapWidth, mapHeight), new Span<uint>(ptr, mapHeight));
            span[x,y] = val;
        }
    }

    public Bitmap CloneCrateMap()
    {
        fixed(uint* ptr = &mapCrate[0])
        {
            var src = new BitmapSpan(new VectorInt2(mapWidth, mapHeight), new Span<uint>(ptr, mapHeight));
            var dest = new Bitmap(mapWidth, mapHeight);
            src.CopyTo(dest);
            return dest;
        }
    }

    public Bitmap CloneMoveMap()
    {
        fixed(uint* ptr = &mapMove[0])
        {
            var src = new BitmapSpan(new VectorInt2(mapWidth, mapHeight), new Span<uint>(ptr, mapHeight));
            var dest = new Bitmap(mapWidth, mapHeight);
            src.CopyTo(dest);
            return dest;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetMoveMapAt(byte x, byte y)
    {
        Debug.Assert(y < MaxMapHeight);
        Debug.Assert(x < MaxMapHeight);

        return (mapMove[y] & ((int)1 << (int)x)) > 0;
    }

    public void SetMapSize(int width, int height)
    {
        Debug.Assert(height < MaxMapHeight);
        mapWidth = (byte)width;
        mapHeight = (byte)height;
    }

    public void SetCrateMap(ref NodeStruct copy)
    {
        Debug.Assert(mapHeight > 0);
        Debug.Assert(mapWidth > 0);
        fixed(uint* dest = &mapCrate[0])
        {
            fixed(uint* src = &copy.mapCrate[0])
            {
                for (int i = 0; i < mapHeight; i++)
                {
                    dest[i] = src[i];
                }
            }
        }
    }

    public void SetCrateMap(IBitmap map)
    {
        fixed(uint* ptr = &mapCrate[0])
        {
            var span = new BitmapSpan(new VectorInt2(mapWidth, mapHeight), new Span<uint>(ptr, mapHeight));
            span.Set(map);
        }
    }
    public void SetMoveMap(IBitmap map)
    {
        fixed(uint* ptr = &mapMove[0])
        {
            var span = new BitmapSpan(new VectorInt2(mapWidth, mapHeight), new Span<uint>(ptr, mapHeight));
            span.Set(map);
        }
    }

    public override string ToString()
    {
        return $"nodeid={nodeid}, parentid={parentid}, player=({playerX}, {playerY}, {playerPushX}, {playerPushY}), hash={hashCode}";
    }

    public string ToDebugString()
    {
        Debug.Assert(mapWidth > 0);
        Debug.Assert(mapHeight > 0);
        var sb = new StringBuilder();

        sb.AppendLine($"{ToStr(parentid)}<-{ToStr(nodeid)}<-{ToStr(firstChildId)} <-> {ToStr(siblingNextId)} Hash:{hashCode}");
        var textMap = new char[mapWidth, mapHeight];
        for(byte y=0; y<mapHeight; y++)
        {
            for(byte x=0; x<mapWidth; x++)
            {
                textMap[x,y] = '.';
                if (GetCrateMapAt(x,y)) textMap[x,y] = 'c';
                if (GetMoveMapAt(x,y)) textMap[x,y] = 'm';
                if (x == playerX && y == playerY) textMap[x,y] = 'P';
                sb.Append(textMap[x,y]);
            }
            sb.AppendLine();
        }
        sb.AppendLine("-----------------------");

        return sb.ToString();

        static string ToStr(uint n)
        {
            if (n == uint.MaxValue) return "(null)";
            return n.ToString();
        }
    }

    public void GenerateMoveMapAndHash(IBitmap wallMap)
    {
        var fillConstraints = new BitmapSpan(wallMap.Size, stackalloc uint[mapHeight]);

        var size = new VectorInt2(mapWidth, mapHeight);
        fixed(uint* ptrMove = &mapMove[0])
        {
            var spanMove = new BitmapSpan(size, new Span<uint>(ptrMove, mapHeight));


            fixed(uint* ptrCrate = &mapCrate[0])
            {
                var spanCrate = new BitmapSpan(size, new Span<uint>(ptrCrate, mapHeight));
                fillConstraints.SetBitwiseOR(spanCrate, wallMap);

                FloodFill.FillRecursiveOptimised(fillConstraints, playerX, playerY, spanMove);
            }
        }

    }

}



