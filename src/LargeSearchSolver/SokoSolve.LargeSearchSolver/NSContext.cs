using System.Diagnostics;
using SokoSolve.Primitives;

namespace SokoSolve.LargeSearchSolver;

public class NSContext
{
    readonly BitmapMaskedState maskState;

    public NSContext(IBitmap mask)
    {
        Width = (byte)mask.Width;
        Height = (byte)mask.Height;
        if (!CanSupportSize(mask))
        {
            throw new NotSupportedException("Too big");
        }
        maskState = BitmapMaskedState.Create(mask);
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
    BitmapMaskedSpan ToBitmapMoveMap(ref NodeStruct node) => new BitmapMaskedSpan(this.maskState, ToSpanMoveMap(ref node));

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
    BitmapMaskedSpan ToBitmapCrateMap(ref NodeStruct node) => new BitmapMaskedSpan(this.maskState, ToSpanCrateMap(ref node));

    public bool GetCrateMapAt(ref NodeStruct nodeStruct, byte x, byte y) => ToBitmapCrateMap(ref nodeStruct)[x,y];
    public bool GetMoveMapAt(ref NodeStruct nodeStruct, byte x, byte y)  => ToBitmapMoveMap(ref nodeStruct)[x,y];

    public void SetCrateMapAt(ref NodeStruct nodeStruct, byte x, byte y, bool val)
    {
        var b = ToBitmapCrateMap(ref nodeStruct);
        b[x,y] = val;
    }

    public void SetMoveMapAt(ref NodeStruct nodeStruct, byte x, byte y, bool val)
    {
        var b = ToBitmapMoveMap(ref nodeStruct);
        b[x,y] = val;
    }

    internal void SetCrateMap(ref NodeStruct dest, IReadOnlyBitmap src) => ToBitmapCrateMap(ref dest).SetFrom(src);
    internal void SetCrateMap(ref NodeStruct dest, ref NodeStruct src)
    {
        // TODO: Span copy
        var d = ToBitmapCrateMap(ref dest);
        var s = ToBitmapCrateMap(ref src);
        d.SetFrom(s);
    }

    internal void SetMoveMap(ref NodeStruct dest, IReadOnlyBitmap src) => ToBitmapMoveMap(ref dest).SetFrom(src);
    internal void CopyCrateMapTo(ref NodeStruct src, IBitmap dest) => ToBitmapCrateMap(ref src).WriteTo(dest);
    internal void CopyMoveMapTo(ref NodeStruct src, IBitmap dest) => ToBitmapMoveMap(ref src).WriteTo(dest);


    internal void GenerateMovemapAndHash(ref NodeStruct nodeStruct, Bitmap wallMap)
    {
        var fillConstraints = new Bitmap(wallMap.Width, wallMap.Height); // TODO: stackalloc?

        var spanMove =  ToBitmapMoveMap(ref nodeStruct);
        var spanCrate =  ToBitmapCrateMap(ref nodeStruct);

        SetBitwiseOR(fillConstraints, spanCrate, wallMap);
        FillRecursive(fillConstraints, nodeStruct.PlayerX, nodeStruct.PlayerY, ref spanMove);

        static void SetBitwiseOR(Bitmap dest, BitmapMaskedSpan lhs, IReadOnlyBitmap rhs)
        {
            for (var cy = 0; cy < dest.Height; cy++)
            {
                for (var cx = 0; cx < dest.Width; cx++)
                    dest[cx, cy] = lhs[cx, cy] || rhs[cx, cy];
            }
        }
        static void FillRecursive(IReadOnlyBitmap constraints, int x, int y, ref BitmapMaskedSpan result)
        {
            if (constraints[x, y]) return;
            if (result[x, y]) return;

            result[x, y] = true;

            if (y > 0)                  FillRecursive(constraints, x, y-1, ref result);
            if (y < constraints.Height) FillRecursive(constraints, x, y+1, ref result);
            if (x > 0)                  FillRecursive(constraints, x-1, y, ref result);
            if (x < constraints.Width)  FillRecursive(constraints, x+1, y, ref result);
        }
    }


    internal bool AllCratesMatch(ref NodeStruct node, IReadOnlyBitmap goalMap)
    {
        // Subtle possible error: all crates most be on a goal (not all goals have a crate on them)!
        // Which is most correct?
        return ToBitmapCrateMap(ref node).IsBitwiseANDMatch(goalMap);
    }
}



