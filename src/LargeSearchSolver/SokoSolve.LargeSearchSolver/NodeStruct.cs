using NodeStructWord = ushort;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using SokoSolve.Primitives;
using SokoSolve.LargeSearchSolver.Utils;
using System.Runtime.InteropServices;

namespace SokoSolve.LargeSearchSolver;

public enum NodeStatus
{
    ALLOC,
    LEASED,
    EVAL_START,
    EVAL_END,
    EVAL_KIDS,
    NEW_CHILD,
    COMPLETE_LEAF,
    DUPLICATE,
    COMPLETE,
}

[StructLayout(LayoutKind.Sequential, Pack=1)]
public unsafe struct NodeStruct
{
    public const int MaxMapHeight = 16;
    public const int MaxMapWidth = sizeof(NodeStructWord) * 8; // bytes to bits

    uint nodeid;
    uint parentid;
    int hashCode;

    byte playerX;
    byte playerY;
    sbyte playerPushX;
    sbyte playerPushY;

    byte mapWidth;      // width,height are not strictly needed, but stops having to pass in the sizes each time the map is read
    byte mapHeight;
    byte status;
    byte type;      // 0 - fwd, 1 - rev

    fixed NodeStructWord mapCrate[MaxMapHeight];
    fixed NodeStructWord mapMove[MaxMapHeight];

    // uint firstChildId; // avoid array of children
    // uint siblingNextId;

    public NodeStruct()
    {
        Reset();
    }

    public static string DescibeMemoryLimits()
    {
        unsafe
        {
            var memNodes = OSHelper.GetAvailableMemory();
            return $"sizeof({nameof(NodeStruct)})={sizeof(NodeStruct)}. TheorticalNodeLimit={memNodes/sizeof(NodeStruct):#,##0}. sizeof(NodeStructWord)={sizeof(NodeStructWord)}";
        }
    }

    public static string Describe() => "v1.1:Nested-MyBitmapStruct,CustomFloodFill";

    public readonly uint NodeId => nodeid;
    public readonly uint ParentId => parentid;
    public readonly int  HashCode => hashCode;
    public readonly NodeStatus Status => (NodeStatus)status;
    public readonly byte PlayerX => playerX;
    public readonly byte PlayerY => playerY;
    public readonly sbyte PlayerPushX => playerPushX;
    public readonly sbyte PlayerPushY => playerPushY;
    public readonly byte Width => mapWidth;
    public readonly byte Height => mapHeight;
    public readonly byte Type => 0;

    public const uint NodeId_NonPooled = uint.MaxValue-1;
    public const uint NodeId_NULL = uint.MaxValue;
    public static bool IsValidId(uint id)
    {
        if (id == NodeId_NULL) return false;
        if (id == NodeId_NonPooled) return false;
        return true;
    }

#region NotCurrentlyUsed
    public readonly uint FirstChildId => NodeId_NULL;
    public readonly uint SiblingNextId => NodeId_NULL ;
    public void SetFirstChildId(uint id) {}
    public void SetSiblingNextId(uint id)  {}
#endregion // NotCurrentlyUsed

    public NodeStructWord GetMapLineCrate(int idx) => mapCrate[idx];
    public NodeStructWord GetMapLineMove(int idx) => mapMove[idx];
    public void SetParent(uint id) => parentid = id;
    public void SetNodeId(uint id) => nodeid = id;
    public void SetHashCode(int code) => hashCode = code;
    public void SetStatus(NodeStatus t) => status = (byte)t;
    public void SetPlayer(byte x, byte y) { playerX = x; playerY = y; }
    public void SetPlayerPush(sbyte dX, sbyte dY) { playerPushX = dX; playerPushY = dY; }
    public void SetType(byte t)  { type = t;}
    public void SetMapSize(int width, int height)
    {
        Debug.Assert(width <= MaxMapWidth);
        Debug.Assert(height <= MaxMapHeight);
        mapWidth = (byte)width;
        mapHeight = (byte)height;
    }

    public void Reset()
    {
        nodeid = NodeId_NULL;
        parentid = NodeId_NULL;
        hashCode = 0;
        // firstChildId = NodeId_NULL;
        // siblingNextId = NodeId_NULL;
        // type = 0;
        playerX = 0;
        playerY = 0;
        playerPushX = 0;
        playerPushY = 0;
        mapWidth = 0;
        mapHeight = 0;
    }

    public void SetFromNode(ref NodeStruct src)
    {
        // nodeid = src.nodeid;  // NEVEr
        parentid = src.parentid;
        // firstChildId = src.firstChildId;
        // siblingNextId = src.siblingNextId;
        // type = src.type;
        playerX = src.playerX;
        playerY = src.playerY;
        playerPushX = src.playerPushX;
        playerPushY = src.playerPushY;
        hashCode = src.hashCode;
        status = src.status;
        mapWidth = src.mapWidth;
        mapHeight = src.mapHeight;
        for(int cc=0; cc<mapHeight; cc++)
        {
            mapCrate[cc] = src.mapCrate[cc];
            mapMove[cc] = src.mapMove[cc];
        }
    }

    public bool EqualsByRef(ref NodeStruct rhs)
    {
        for(int cc=0; cc<mapHeight; cc++)
        {
            if (mapCrate[cc] != rhs.mapCrate[cc]) return false;
            if (mapMove[cc] != rhs.mapMove[cc]) return false;
        }
        return true;
    }

    public int CompareByRef(ref NodeStruct rhs)
    {
        for(int cc=0; cc<mapHeight; cc++)
        {
            var cmpC = mapCrate[cc].CompareTo(rhs.mapCrate[cc]);
            if (cmpC != 0) return cmpC;

            var cmpM = mapMove[cc].CompareTo(rhs.mapMove[cc]);
            if (cmpM != 0) return cmpM;
        }
        return 0;
    }

    public readonly override int GetHashCode() => hashCode;
    public bool Equals(NodeStruct rhs) => EqualsByRef(ref rhs);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is NodeStruct ns)
        {
            return EqualsByRef(ref ns);
        }
        throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetCrateMapAt(byte x, byte y)
    {
        return (mapCrate[y] & ((int)1 << (int)x)) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetMoveMapAt(byte x, byte y)
    {
        return (mapMove[y] & ((int)1 << (int)x)) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCrateMapAt(byte x, byte y, bool val)
    {
        fixed(NodeStructWord* ptr = &mapCrate[0])
        {
            var span = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptr, mapHeight));
            span[x,y] = val;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMoveMapAt(byte x, byte y, bool val)
    {
        fixed(NodeStructWord* ptr = &mapMove[0])
        {
            var span = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptr, mapHeight));
            span[x,y] = val;
        }
    }

    public void SetCrateMap(ref NodeStruct copy)
    {
        fixed(NodeStructWord* dest = &mapCrate[0])
        {
            fixed(NodeStructWord* src = &copy.mapCrate[0])
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
        fixed(NodeStructWord* ptr = &mapCrate[0])
        {
            var span = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptr, mapHeight));
            span.Set(map);
        }
    }

    public void SetMoveMap(IBitmap map)
    {
        fixed(NodeStructWord* ptr = &mapMove[0])
        {
            var span = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptr, mapHeight));
            span.Set(map);
        }
    }

    public void GenerateMoveMapAndHash(Bitmap wallMap)
    {
        var fillConstraints = new MyBitmapSpan(mapWidth, mapHeight, stackalloc NodeStructWord[mapHeight]);

        fixed(NodeStructWord* ptrMove = &mapMove[0])
        {
            var spanMove = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptrMove, mapHeight));

            fixed(NodeStructWord* ptrCrate = &mapCrate[0])
            {
                var spanCrate = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptrCrate, mapHeight));
                fillConstraints.SetBitwiseOR(spanCrate, wallMap);
                FillRecursive(fillConstraints, playerX, playerY, spanMove);
            }
        }
    }

    static void FillRecursive(MyBitmapSpan constraints, int x, int y, MyBitmapSpan result)
    {
        if (constraints[x, y]) return;
        if (result[x, y]) return;

        result[x, y] = true;

        if (y > 0) FillRecursive(constraints, x, y-1, result);
        if (y < constraints.Height) FillRecursive(constraints, x, y+1, result);
        if (x > 0) FillRecursive(constraints, x-1, y, result);
        if (x < constraints.Width) FillRecursive(constraints, x+1, y, result);
    }

    public override readonly string ToString()
    {
        return $"{ToStr(parentid)}<-{ToStr(nodeid)}<- Status({Status}) Hash:{hashCode}";
    }

    public string ToDebugString()
    {
        Debug.Assert(mapWidth > 0);
        Debug.Assert(mapHeight > 0);
        var sb = new StringBuilder();

        sb.AppendLine($"{ToStr(parentid)}<-{ToStr(nodeid)}<- Hash:{hashCode}");
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

    }

    static string ToStr(uint n)
    {
        if (n == NodeId_NULL) return "(null)";
        return n.ToString();
    }

    internal bool AllCratesMatch(Bitmap goalMap)
    {
        fixed(NodeStructWord* ptrCrate = &mapCrate[0])
        {
            var spanCrate = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptrCrate, mapHeight));
            return spanCrate.IsBitwiseANDMatch(goalMap);
        }
    }

    readonly ref struct MyBitmapSpan // IBitmap
    {
        readonly Span<NodeStructWord> map;
        readonly byte width;
        readonly byte height;

        public MyBitmapSpan(byte width, byte height, Span<NodeStructWord> map)
        {
            this.width = width;
            this.height = height;
            this.map = map;
        }

        public byte Width => width;
        public byte Height => height;

        public bool this[int pX, int pY]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (map[pY] & (1 << pX)) > 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => map[pY] = value
                ? (NodeStructWord)(map[pY] |  (1 << pX))
                : (NodeStructWord)(map[pY] & ~(1 << pX));
        }

        public void Set(IBitmap source)
        {
            for (var cy = 0; cy < height; cy++)
            {
                for (var cx = 0; cx < width; cx++)
                    this[cx, cy] = source[cx, cy];
            }
        }

        public void SetBitwiseOR(MyBitmapSpan a, Bitmap b)
        {
            for (var cy = 0; cy < height; cy++)
            {
                map[cy] =(NodeStructWord)(a.map[cy] | (NodeStructWord)b[cy]);
            }
        }

        internal bool IsBitwiseANDMatch(Bitmap goalMap)
        {
            for (var cy = 0; cy < height; cy++)
            {
                var aa = map[cy];
                if ((aa & (NodeStructWord)goalMap[cy]) != aa) return false;
            }
            return true;
        }
    }
}


