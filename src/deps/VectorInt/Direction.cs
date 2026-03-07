using System.Collections.Generic;
using System.IO;

namespace VectorInt;

public readonly struct Direction
{
    byte dir;

    private Direction(byte dir)
    {
        this.dir = dir;
    }

    public Direction(VectorInt2 d)
    {
        for(byte idx=0; idx<ToVectByIndex.Length; idx++)
        {
            if (ToVectByIndex[idx] == d)
            {
                dir = idx;
                return;
            }
        }
        throw new InvalidDataException(d.ToString());
    }

    public static readonly Direction Left  = new Direction(0);
    public static readonly Direction Right = new Direction(1);
    public static readonly Direction Up    = new Direction(2);
    public static readonly Direction Down  = new Direction(3);

    static readonly VectorInt2[] ToVectByIndex =
        [
            new VectorInt2(+1, 0), // right
            new VectorInt2(-1, 0), // left
            new VectorInt2(0, -1), // up
            new VectorInt2(0, 1), // down
        ];

    public static VectorInt2 operator+ (VectorInt2 v, Direction d) => v + ToVectByIndex[d.dir];
    public static VectorInt2 operator- (VectorInt2 v, Direction d) => v - ToVectByIndex[d.dir];
}

public class PathDirection : List<Direction>
{
    public PathDirection()
    {
    }

    public PathDirection(IEnumerable<Direction> collection) : base(collection)
    {
    }

    public IEnumerable<VectorInt2> Follow(VectorInt2 start)
    {
        var p = start;
        yield return p;
        foreach(var d in this)
        {
            p += d;
            yield return p;
        }
    }
}
