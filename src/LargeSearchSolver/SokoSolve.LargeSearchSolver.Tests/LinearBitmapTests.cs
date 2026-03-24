using SokoSolve.Primitives;
using VectorInt;
namespace SokoSolve.LargeSearchSolver.Tests;

public class LinearBitmapTests
{
    [Fact]
    public void SizeComparison()
    {
        var b = new Bitmap(14,16);
        Assert.Equal(16*4, b.SizeInBytes());

        var l = new LinearBitmap(14, 16);
        Assert.Equal(14*16/8, l.SizeInBytes());

        Assert.True(b.SizeInBytes() > l.SizeInBytes());

        Console.WriteLine($"Size: {l.SizeInBytes()*100/b.SizeInBytes()}%");
    }

    [Fact]
    public void Mask()
    {
        var floor = PuzzleLibraryStatic.PQ1_P29.ToMap(PuzzleLibraryStatic.PQ1_P29.Definition.AllFloors);
        Console.WriteLine(floor);

        int[,] index = new int[floor.Width, floor.Height];
        List<VectorInt2> toPos = new();
        var cc = 0;
        foreach(var p in floor.ForEach())
        {
            if (p.val)
            {
                index[p.pos.X, p.pos.Y] = cc++;
                toPos.Add(p.pos);
            }
            else
            {
                index[p.pos.X, p.pos.Y] = -1;
            }
        }

        // Console.WriteLine($"Saving: {cc*100f/(floor.Size.X*floor.Size.Y)}%");
        // for(int x=0; x<toPos.Count; x++)
        // {
        //     Console.WriteLine($"Idx: {x} => {toPos[x]} <== {index[toPos[x].X, toPos[x].Y]}");
        // }
    }
}




