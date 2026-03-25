using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

[StructLayout(LayoutKind.Sequential, Pack=1)]
public unsafe struct NodeStruct
{
    public const int MaxMapBuffer = 10; // 10*8=80 bits of precision

    // required
    uint nodeid;
    uint parentid;
    int hashCode;

    // idea: refactor this to a linear bitmap
    internal fixed byte mapCrate[MaxMapBuffer];
    internal fixed byte mapMove[MaxMapBuffer];

    // byte status;
    // byte type;      // 0 - fwd, 1 - rev
    // sbyte playerPushX;
    // sbyte playerPushY;
    byte type_status_playerpush;

    // optional (could be factored away)
    byte playerX;       // idea: move playerx,y,pushx,pushy into a wrapper class (as they are mainly used outside of the nodeheap)
    byte playerY;

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
            return $"sizeof({nameof(NodeStruct)})={sizeof(NodeStruct)}. TheorticalNodeLimit={memNodes/sizeof(NodeStruct):#,##0}";
        }
    }

    public static string Describe() => "v2.1:CustomFloodFill,BitPacking,NSContext,BitmapMasked";

    // primary
    public readonly uint NodeId => nodeid;
    public readonly uint ParentId => parentid;
    public readonly int  HashCode => hashCode;

    public readonly NodeStatus Status => (NodeStatus)BitsTypeStatusPlayerPush.GetStatus(type_status_playerpush);
    public readonly byte Type => (byte)BitsTypeStatusPlayerPush.GetType(type_status_playerpush);
    public readonly sbyte PlayerPushX
    {
        get
        {
            var b = (byte)BitsTypeStatusPlayerPush.GetPushDir(type_status_playerpush);
            var dir = Direction.FromByte(b);
            if (dir.Equals(Direction.Left)) return -1;
            if (dir.Equals(Direction.Right)) return 1;
            return 0;
        }
    }
    public readonly sbyte PlayerPushY
    {
        get
        {
            var b = (byte)BitsTypeStatusPlayerPush.GetPushDir(type_status_playerpush);
            var dir = Direction.FromByte(b);
            if (dir.Equals(Direction.Up)) return -1;
            if (dir.Equals(Direction.Down)) return 1;
            return 0;
        }
    }

    public readonly byte PlayerX => playerX;
    public readonly byte PlayerY => playerY;

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

    public ReadOnlySpan<byte> GetBufferMoveMap()
    {
        unsafe
        {
            fixed(byte* ptr = &mapMove[0])
            {
                return new ReadOnlySpan<byte>(ptr, NodeStruct.MaxMapBuffer);
            }
        }
    }
    public ReadOnlySpan<byte> GetBufferCrateMap()
    {
        unsafe
        {
            fixed(byte* ptr = &mapCrate[0])
            {
                return new ReadOnlySpan<byte>(ptr, NodeStruct.MaxMapBuffer);
            }
        }
    }

#region NotCurrentlyUsed
    public readonly uint FirstChildId => NodeId_NULL;
    public readonly uint SiblingNextId => NodeId_NULL ;
    public void SetFirstChildId(uint id) {}
    public void SetSiblingNextId(uint id)  {}
#endregion // NotCurrentlyUsed

    public void SetParent(uint id) => parentid = id;
    public void SetNodeId(uint id) => nodeid = id;
    public void SetHashCode(int code) => hashCode = code;

    public void SetType(byte t)
    {
        type_status_playerpush =  BitsTypeStatusPlayerPush.SetType(type_status_playerpush, t);
    }
    public void SetStatus(NodeStatus t)
    {
        type_status_playerpush = BitsTypeStatusPlayerPush.SetStatus(type_status_playerpush, (int)t);
    }
    public void SetPlayerPush(sbyte dX, sbyte dY)
    {
        var dir = new Direction(new VectorInt2(dX, dY));
        type_status_playerpush = BitsTypeStatusPlayerPush.SetPushDir(type_status_playerpush, dir.ToByte());
    }

    public void SetPlayer(int x, int y) { playerX = (byte)x; playerY = (byte)y; }
    public void SetPlayer(byte x, byte y) { playerX = x; playerY = y; }
    public void SetPlayerPush(int dX, int dY) => SetPlayerPush((sbyte)dX, (sbyte)dY);
    public void SetTypeReverse()  { SetType(NodeType_Reverse);}

    public void Reset()
    {
        nodeid = NodeId_NULL;
        parentid = NodeId_NULL;
        hashCode = 0;
        playerX = 0;
        playerY = 0;
        type_status_playerpush = 0;     // status = 0
        // firstChildId = NodeId_NULL;
        // siblingNextId = NodeId_NULL;
        // type = 0;
    }

    public void SetFromNode(NSContext ctx,ref NodeStruct src)
    {
        // nodeid = src.nodeid;  // NEVEr
        parentid = src.parentid;
        // firstChildId = src.firstChildId;
        // siblingNextId = src.siblingNextId;
        playerX = src.playerX;
        playerY = src.playerY;
        hashCode = src.hashCode;

        SetType(src.Type);
        SetStatus(src.Status);
        SetPlayerPush(src.PlayerPushX, src.PlayerPushY);

        // Copy maps
        for(int cc=0; cc<ctx.Height; cc++)
        {
            mapCrate[cc] = src.mapCrate[cc];
            mapMove[cc] = src.mapMove[cc];
        }
    }

    public bool EqualsByRef(NSContext ctx,ref NodeStruct rhs)
    {
        for(int cc=0; cc<ctx.Height; cc++)
        {
            if (mapCrate[cc] != rhs.mapCrate[cc]) return false;
            if (mapMove[cc] != rhs.mapMove[cc]) return false;
        }
        return true;
    }

    public int CompareByRef(NSContext ctx, ref NodeStruct rhs)
    {
        for(int cc=0; cc<ctx.Height; cc++)
        {
            var cmpC = mapCrate[cc].CompareTo(rhs.mapCrate[cc]);
            if (cmpC != 0) return cmpC;

            var cmpM = mapMove[cc].CompareTo(rhs.mapMove[cc]);
            if (cmpM != 0) return cmpM;
        }
        return 0;
    }

    public readonly override int GetHashCode() => hashCode;
    public bool Equals(NodeStruct rhs) => throw new NotSupportedException();

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        throw new NotSupportedException("Use EqualsByRef with NSContext instead");
    }

    public bool GetCrateMapAt(NSContext ctx, byte x, byte y)           => ctx.GetCrateMapAt(ref this, x, y);
    public bool GetMoveMapAt(NSContext ctx, byte x, byte y)            => ctx.GetMoveMapAt(ref this, x, y);
    public void SetCrateMapAt(NSContext ctx, byte x, byte y, bool val) => ctx.SetCrateMapAt(ref this, x, y, val);
    public void SetMoveMapAt(NSContext ctx,byte x, byte y, bool val)   => ctx.SetMoveMapAt(ref this, x, y, val);
    public void SetCrateMap(NSContext ctx, ref NodeStruct copy)        => ctx.SetCrateMap(ref this, ref copy);
    public void SetCrateMap(NSContext ctx, IReadOnlyBitmap map)        => ctx.SetCrateMap(ref this, map);
    public void SetMoveMap(NSContext ctx, IReadOnlyBitmap map)         => ctx.SetMoveMap(ref this, map);
    public void CopyCrateMapTo(NSContext ctx, IBitmap map)             => ctx.CopyCrateMapTo(ref this, map);
    public void CopyMoveMapTo(NSContext ctx, IBitmap map)              => ctx.CopyMoveMapTo(ref this, map);
    public void GenerateMoveMapAndHash(NSContext ctx,Bitmap wallMap)   => ctx.GenerateMovemapAndHash(ref this, wallMap);

    public override readonly string ToString()
    {
        return $"{ToStr(parentid)}<-{ToStr(nodeid)}<- Status({Status}) Hash:{hashCode}";
    }

    /// <summary>WARNING: changing this format will break unit tests</summary>
    public string ToDebugString(NSContext ctx,  bool incHash = false, LSolverState? state = null)
    {
        Debug.Assert(ctx.Width > 0);
        Debug.Assert(ctx.Height > 0);
        var sb = new StringBuilder();

        var textMap = new char[ctx.Width, ctx.Height];
        for(byte y=0; y<ctx.Height; y++)
        {
            sb.Append("| ");
            for(byte x=0; x<ctx.Width; x++)
            {
                textMap[x,y] = '.';
                if (GetCrateMapAt(ctx, x,y))
                {
                    textMap[x,y] = 'C';
                }
                if (GetMoveMapAt(ctx, x, y))
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
            if (y == 1 && incHash)
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
                sb.Append($" dX:{PlayerPushX}, dY:{PlayerPushY}");
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

    internal bool AllCratesMatch(NSContext ctx, Bitmap goalMap) => ctx.AllCratesMatch(ref this, goalMap);

    internal VectorInt2 GetNewCratePos()
    {
        return new VectorInt2(PlayerX, PlayerY) + new VectorInt2(PlayerPushX, PlayerPushY);
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
    public static bool TryParseDebugText(NSContext ctx, string nodeText, ref NodeStruct node)
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

            // Clear all map positions first
            for (byte y = 0; y < mapHeight; y++)
            {
                for (byte x = 0; x < mapWidth; x++)
                {
                    node.SetCrateMapAt(ctx, x, y, false);
                    node.SetMoveMapAt(ctx, x, y, false);
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
                            node.SetMoveMapAt(ctx, x, y, true);
                            break;
                        case 'C':
                            // Crate (blocks movement)
                            node.SetCrateMapAt(ctx, x, y, true);
                            node.SetMoveMapAt(ctx, x, y, false); // Ensure move is false
                            break;
                        case 'P':
                            // Player at moveable position
                            node.SetMoveMapAt(ctx, x, y, true);
                            playerX = x;
                            playerY = y;
                            break;
                        case 'p':
                            // Player at non-moveable position
                            playerX = x;
                            playerY = y;
                            node.SetMoveMapAt(ctx, x, y, false); // Ensure move is false
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


