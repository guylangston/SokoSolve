using NodeStructWord = ushort;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using SokoSolve.Primitives;
using SokoSolve.LargeSearchSolver.Utils;
using System.Runtime.InteropServices;
using VectorInt;
using System.Text.RegularExpressions;

namespace SokoSolve.LargeSearchSolver;

public enum NodeStatus
{
    NONE,

    ALLOC,
    LEASED,

    NEW_CHILD,
    DEAD,           // should never be committed
    DUPLICATE,      // should never be committed

    COMPLETE,
    COMPLETE_LEAF,

    // These should also be a separate from  above
    CHAIN,
    SOLUTION,

    // TODO: move to threadstate
    EVAL_START,
    EVAL_END,
    EVAL_KIDS,
}

[StructLayout(LayoutKind.Sequential, Pack=1)]
public unsafe struct NodeStruct
{
    public const int MaxMapHeight = 14;
    public const int MaxMapWidth = sizeof(NodeStructWord) * 8; // bytes to bits

    // required
    uint nodeid;
    uint parentid;
    int hashCode;
    byte status;
    byte type;      // 0 - fwd, 1 - rev

    // optional (could be factored away)
    byte mapWidth;      // width,height are not strictly needed, but stops having to pass in the sizes each time the map is read
    byte mapHeight;
    byte playerX;       // idea: move playerx,y,pushx,pushy into a wrapper class (as they are mainly used outside of the nodeheap)
    byte playerY;
    sbyte playerPushX;
    sbyte playerPushY;

    // idea: refactor this to a linear bitmap
    fixed NodeStructWord mapCrate[MaxMapHeight];
    fixed NodeStructWord mapMove[MaxMapHeight];

    // TODO: re-enable? or add to wrapper
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
    public readonly byte Type => type;

    public const uint NodeId_NonPooled = uint.MaxValue-1;
    public const uint NodeId_NULL = uint.MaxValue;
    public const byte NodeType_Forward = 0;
    public const byte NodeType_Reverse = 1;
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
    public void SetPlayer(int x, int y) { playerX = (byte)x; playerY = (byte)y; }
    public void SetPlayer(byte x, byte y) { playerX = x; playerY = y; }
    public void SetPlayerPush(int dX, int dY) { playerPushX = (sbyte)dX; playerPushY = (sbyte)dY; }
    public void SetPlayerPush(sbyte dX, sbyte dY) { playerPushX = dX; playerPushY = dY; }
    public void SetType(byte t)  { type = t;}
    public void SetTypeReverse()  { type = NodeType_Reverse;}
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
        playerX = 0;
        playerY = 0;
        playerPushX = 0;
        playerPushY = 0;
        mapWidth = 0;
        mapHeight = 0;
        // firstChildId = NodeId_NULL;
        // siblingNextId = NodeId_NULL;
        // type = 0;
    }

    public void SetFromNode(ref NodeStruct src)
    {
        // nodeid = src.nodeid;  // NEVEr
        parentid = src.parentid;
        // firstChildId = src.firstChildId;
        // siblingNextId = src.siblingNextId;
        type = src.type;
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
    public void SetCrateMapAt(int x, int y, bool val) => SetCrateMapAt((byte)x, (byte)y, val);
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

    public void CopyCrateMapTo(IBitmap map)
    {
        fixed(NodeStructWord* ptr = &mapCrate[0])
        {
            var span = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptr, mapHeight));
            span.CopyTo(map);
        }
    }

    public void SetCrateMap(IBitmap map)
    {
        fixed(NodeStructWord* ptr = &mapCrate[0])
        {
            var span = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptr, mapHeight));
            span.SetFrom(map);
        }
    }

    public void CopyMoveMapTo(IBitmap map)
    {
        fixed(NodeStructWord* ptr = &mapMove[0])
        {
            var span = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptr, mapHeight));
            span.CopyTo(map);
        }
    }
    public void SetMoveMap(IBitmap map)
    {
        fixed(NodeStructWord* ptr = &mapMove[0])
        {
            var span = new MyBitmapSpan(mapWidth, mapHeight, new Span<NodeStructWord>(ptr, mapHeight));
            span.SetFrom(map);
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

    /// <summary>WARNING: changing this format will break unit tests</summary>
    public string ToDebugString(LSolverState? state = null)
    {
        Debug.Assert(mapWidth > 0);
        Debug.Assert(mapHeight > 0);
        var sb = new StringBuilder();

        var textMap = new char[mapWidth, mapHeight];
        for(byte y=0; y<mapHeight; y++)
        {
            sb.Append("| ");
            for(byte x=0; x<mapWidth; x++)
            {
                textMap[x,y] = '.';
                if (GetCrateMapAt(x,y))
                {
                    textMap[x,y] = 'C';
                }
                if (GetMoveMapAt(x, y))
                {
                    textMap[x, y] = 'M';
                    if (x == playerX && y == playerY) textMap[x, y] = 'P';
                }
                else if (x == playerX && y == playerY)
                {
                    textMap[x, y] = 'p';
                }

                sb.Append(textMap[x,y]);
            }
            sb.Append(" |");
            if (y == 0)
            {
                sb.Append($" NodeId:{ToStr(nodeid)} -> ParentId:{ToStr(parentid)}");
            }
            if (y == 1)
            {
                string stable = "stability?";
                if (state != null)
                {
                    stable = state.HashCalculator.IsStable ? "stable" : "UNSTABLE";
                }
                sb.Append($" #{hashCode} {stable}");
            }
            if (y == 2)
            {
                sb.Append(Type == NodeType_Forward ? " FWD" : " REV");
            }
            if (y == 3)
            {
                sb.Append(' ');
                sb.Append(Status);
            }
            if (y == 4)
            {
                sb.Append($" dX:{playerPushX}, dY:{playerPushY}");
            }
            if (y == 5)
            {
                // Solution { SolutionFwd, SolutionRec, SolutionChain, SolutionPath}
                if (state != null)
                {
                    if (state.SolutionsForward.Contains(nodeid))
                    {
                        sb.Append(" Solution(Fwd)");
                    }
                    if (state.SolutionsReverse.Contains(nodeid))
                    {
                        sb.Append(" Solution(Rev)");
                    }
                    var nn = nodeid;
                    if (state.SolutionsChain.Any(x=>x.chainReverseId == nn))
                    {
                        sb.Append(" Solution(Chain)");
                    }
                }
            }
            sb.AppendLine();
        }

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

    internal VectorInt2 GetNewCratePos()
    {
        return new VectorInt2(PlayerX, PlayerY) + new VectorInt2(PlayerPushX, PlayerPushY);
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

        public void SetFrom(IBitmap source)
        {
            for (var cy = 0; cy < height; cy++)
            {
                for (var cx = 0; cx < width; cx++)
                    this[cx, cy] = source[cx, cy];
            }
        }

        public void CopyTo(IBitmap source)
        {
            for (var cy = 0; cy < height; cy++)
            {
                for (var cx = 0; cx < width; cx++)
                    source[cx, cy] = this[cx, cy];
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

    //
    // Example:
    // var nodeText =
    //     """
    //     | ........... | NodeId:1 -> ParentId:0
    //     | ....M...... | #-609065677 stability?
    //     | ...MM...MM. | FWD
    //     | ..MPMMMMMM. | COMPLETE
    //     | .MMCMCM.MM. | dX:0, dY:1
    //     | ...MC...MM. |
    //     | ...MM.MMMM. |
    //     | ...M..M.M.. |
    //     | ..MMMMMM... |
    //     | ..MMMMM.... |
    //     | ........... |
    //     """;
    // Note: P should also set MoveMap=true
    public static bool TryParseDebugText(string nodeText, ref NodeStruct node)
    {
        try
        {
            var lines = nodeText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return false;

            uint nodeId = 0, parentId = 0;
            int hashCode = 0;
            byte type = NodeStruct.NodeType_Forward;
            NodeStatus status = NodeStatus.ALLOC;
            sbyte playerPushX = 0, playerPushY = 0;
            byte playerX = 0, playerY = 0;

            var mapLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("|")) continue;

                // Find the second pipe to extract the map part
                var secondPipe = trimmed.IndexOf('|', 1);
                if (secondPipe < 0) continue;

                // Extract map content between first and second pipe
                var mapPart = trimmed.Substring(1, secondPipe - 1).Trim();
                mapLines.Add(mapPart);

                // Check for metadata after second pipe
                if (secondPipe + 1 < trimmed.Length)
                {
                    var metadata = trimmed.Substring(secondPipe + 1).Trim();
                    if (string.IsNullOrEmpty(metadata)) continue;

                    // Parse NodeId and ParentId
                    if (metadata.Contains("NodeId:"))
                    {
                        var nodeIdMatch = Regex.Match(metadata, @"NodeId:(\d+)");
                        var parentIdMatch = Regex.Match(metadata, @"ParentId:(\d+)");
                        if (nodeIdMatch.Success) nodeId = uint.Parse(nodeIdMatch.Groups[1].Value);
                        if (parentIdMatch.Success) parentId = uint.Parse(parentIdMatch.Groups[1].Value);
                    }
                    // Parse HashCode
                    else if (metadata.StartsWith("#"))
                    {
                        var hashMatch = Regex.Match(metadata, @"#(-?\d+)");
                        if (hashMatch.Success) hashCode = int.Parse(hashMatch.Groups[1].Value);
                    }
                    // Parse Type
                    else if (metadata == "FWD")
                    {
                        type = NodeStruct.NodeType_Forward;
                    }
                    else if (metadata == "REV")
                    {
                        type = NodeStruct.NodeType_Reverse;
                    }
                    // Parse Status
                    else if (Enum.TryParse<NodeStatus>(metadata, out var parsedStatus))
                    {
                        status = parsedStatus;
                    }
                    // Parse PlayerPush
                    else if (metadata.Contains("dX:"))
                    {
                        var dxMatch = Regex.Match(metadata, @"dX:(-?\d+)");
                        var dyMatch = Regex.Match(metadata, @"dY:(-?\d+)");
                        if (dxMatch.Success) playerPushX = sbyte.Parse(dxMatch.Groups[1].Value);
                        if (dyMatch.Success) playerPushY = sbyte.Parse(dyMatch.Groups[1].Value);
                    }
                }
            }

            if (mapLines.Count == 0) return false;

            // Determine map dimensions
            byte mapWidth = (byte)mapLines[0].Length;
            byte mapHeight = (byte)mapLines.Count;

            // Initialize node
            node.Reset();
            node.SetNodeId(nodeId);
            node.SetParent(parentId);
            node.SetHashCode(hashCode);
            node.SetType(type);
            node.SetStatus(status);
            node.SetPlayerPush(playerPushX, playerPushY);
            node.SetMapSize(mapWidth, mapHeight);

            // Clear all map positions first
            for (byte y = 0; y < mapHeight; y++)
            {
                for (byte x = 0; x < mapWidth; x++)
                {
                    node.SetCrateMapAt(x, y, false);
                    node.SetMoveMapAt(x, y, false);
                }
            }

            // Parse the map grid
            for (byte y = 0; y < mapHeight; y++)
            {
                var mapLine = mapLines[y];
                for (byte x = 0; x < mapLine.Length; x++)
                {
                    var ch = mapLine[x];
                    switch (ch)
                    {
                        case 'M':
                            // Moveable position (no crate)
                            node.SetMoveMapAt(x, y, true);
                            break;
                        case 'C':
                            // Crate (blocks movement)
                            node.SetCrateMapAt(x, y, true);
                            node.SetMoveMapAt(x, y, false); // Ensure move is false
                            break;
                        case 'P':
                            // Player at moveable position
                            node.SetMoveMapAt(x, y, true);
                            playerX = x;
                            playerY = y;
                            break;
                        case 'p':
                            // Player at non-moveable position
                            playerX = x;
                            playerY = y;
                            node.SetMoveMapAt(x, y, false); // Ensure move is false
                            break;
                        case '.':
                            // Empty space - default, no action needed
                            break;
                    }
                }
            }

            node.SetPlayer(playerX, playerY);

            return true;
        }
        catch
        {
            return false;
        }
    }
}


