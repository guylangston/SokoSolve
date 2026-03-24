using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorInt;

public class DirectionPath : List<Direction>
{
    public DirectionPath()
    {
    }

    public DirectionPath(IEnumerable<Direction> collection) : base(collection)
    {
    }

    public DirectionPath(string pathStr) : this(
        pathStr.ToUpperInvariant()
            .Where(x => "UDLR".Contains(x))
            .Select(x => new Direction(char.ToUpperInvariant(x))))
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


    public override string ToString()
    {
        var sb = new StringBuilder();
        var cc = 0;
        foreach (var dir in this)
        {
            sb.Append(dir.ToChar());
            if (cc++ > 80)
            {
                sb.AppendLine();
                cc = 0;
            }
        }

        return sb.ToString();
    }
}

