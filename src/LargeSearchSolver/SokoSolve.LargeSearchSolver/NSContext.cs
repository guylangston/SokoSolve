using SokoSolve.Primitives;

namespace SokoSolve.LargeSearchSolver;

public class NSContext
{
    readonly BitmapMaskedState maskState;

    public NSContext(int width, int height, IBitmap floorMask)
    {
        Width = (byte)width;
        Height = (byte)height;
        if (!CanSupportSize(floorMask))
        {
            throw new NotSupportedException("Too big");
        }
        maskState = BitmapMaskedState.Create(floorMask);
    }

    public static bool CanSupportSize(IBitmap floor)
    {
        var avail = NodeStruct.MaxMapBuffer * 8;
        return avail >= floor.Count();
    }

    public byte Width { get; }
    public byte Height { get; }
    // public IBitmap FloorMask { get; }

    Span<byte> ToSpanMoveMap(ref NodeStruct node)
    {
        unsafe
        {
            fixed(byte* ptr = &node.mapMove[0])
            {
                return new Span<byte>(ptr, NodeStruct.MaxMapBuffer);
            }
        }
    }
    Span<byte> ToSpanCrateMap(ref NodeStruct node)
    {
        unsafe
        {
            fixed(byte* ptr = &node.mapCrate[0])
            {
                return new Span<byte>(ptr, NodeStruct.MaxMapBuffer);
            }
        }
    }

    public bool GetCrateMapAt(ref NodeStruct nodeStruct, byte x, byte y)
    {
        var idx = maskState.PositionToIndex[x,y];
        return BitArrayHelper.GetBit(ToSpanCrateMap(ref nodeStruct), idx);
    }

    public bool GetMoveMapAt(ref NodeStruct nodeStruct, byte x, byte y)
    {
        var idx = maskState.PositionToIndex[x,y];
        return BitArrayHelper.GetBit(ToSpanMoveMap(ref nodeStruct), idx);
    }

    public void SetCrateMapAt(ref NodeStruct nodeStruct, byte x, byte y, bool val)
    {
        var idx = maskState.PositionToIndex[x,y];
        BitArrayHelper.SetBit(ToSpanCrateMap(ref nodeStruct), idx, val);
    }

    public void SetMoveMapAt(ref NodeStruct nodeStruct, byte x, byte y, bool val)
    {
        var idx = maskState.PositionToIndex[x,y];
        BitArrayHelper.SetBit(ToSpanMoveMap(ref nodeStruct), idx, val);
    }

    internal void SetCrateMap(ref NodeStruct nodeStruct, IReadOnlyBitmap copy)
    {
        var span = ToSpanCrateMap(ref nodeStruct);
        var bm = new BitmapMaskedSpan(maskState, span);
        bm.SetFrom(copy);
    }

    internal void SetCrateMap(ref NodeStruct nodeStruct, ref NodeStruct copy)
    {
    }

    internal void SetMoveMap(ref NodeStruct nodeStruct, IReadOnlyBitmap map)
    {
        var span = ToSpanMoveMap(ref nodeStruct);
        var bm = new BitmapMaskedSpan(maskState, span);
        bm.SetFrom(map);
    }

    internal void CopyCrateMapTo(ref NodeStruct nodeStruct, IBitmap map)
    {
        throw new NotImplementedException();
    }

    internal void CopyMoveMapTo(ref NodeStruct nodeStruct, IBitmap map)
    {
        throw new NotImplementedException();
    }

    static void FillRecursive(IReadOnlyBitmap constraints, int x, int y, IBitmap result)
    {
        if (constraints[x, y]) return;
        if (result[x, y]) return;

        result[x, y] = true;

        if (y > 0) FillRecursive(constraints, x, y-1, result);
        if (y < constraints.Height) FillRecursive(constraints, x, y+1, result);
        if (x > 0) FillRecursive(constraints, x-1, y, result);
        if (x < constraints.Width) FillRecursive(constraints, x+1, y, result);
    }

    internal void GenerateMovemapAndHash(ref NodeStruct nodeStruct, Bitmap wallMap)
    {
        // var fillConstraints = new MyBitmapSpan(ctx.Width, ctx.Height, stackalloc NodeStructWord[ctx.Height]);
        //
        // fixed(NodeStructWord* ptrMove = &mapMove[0])
        // {
        //     var spanMove = new MyBitmapSpan(ctx.Width, ctx.Height, new Span<NodeStructWord>(ptrMove, ctx.Height));
        //
        //     fixed(NodeStructWord* ptrCrate = &mapCrate[0])
        //     {
        //         var spanCrate = new MyBitmapSpan(ctx.Width, ctx.Height, new Span<NodeStructWord>(ptrCrate, ctx.Height));
        //         fillConstraints.SetBitwiseOR(spanCrate, wallMap);
        //         FillRecursive(fillConstraints, playerX, playerY, spanMove);
        //     }
        // }
    }

    internal bool AllCratesMatch(ref NodeStruct node, Bitmap goalMap)
    {
        // fixed(NodeStructWord* ptrCrate = &mapCrate[0])
        // {
        //     var spanCrate = new MyBitmapSpan(ctx.Width, ctx.Height, new Span<NodeStructWord>(ptrCrate, ctx.Height));
        //     return spanCrate.IsBitwiseANDMatch(goalMap);
        // }
        return false;
    }
}



