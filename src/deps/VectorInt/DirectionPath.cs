using System.Collections.Generic;

namespace VectorInt;

public class DirectionPath : List<Direction>
{
    public DirectionPath()
    {
    }

    public DirectionPath(IEnumerable<Direction> collection) : base(collection)
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

