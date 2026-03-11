using System.Text;
using VectorInt;
using static VectorInt.VectorInt2;

namespace SokoSolve.Primitives.Analytics;

// TODO: Refactor to use VectorInt.Direction
public class Path : List<Direction>
{
    public const char Up = 'U';
    public const char Down = 'D';
    public const char Left = 'L';
    public const char Right = 'R';

    public Path() { }

    public Path(IEnumerable<Direction> collection) : base(collection) {}

    public Path(IEnumerable<VectorInt2> collection)
        : base(collection.Where(x => x != Zero).Select(x=>new Direction(x))) { }

    public Path(string pathStr) : this(
        pathStr.ToUpperInvariant()
            .Where(x => "UDLR".Contains(x))
            .Select(x => new Direction(char.ToUpperInvariant(x))))
    {
    }

    public string? Description  { get; set; }
    public int     NodeDepth    => NodeDepthFwd + NodeDepthRev;
    public int     NodeDepthFwd { get; set; }
    public int     NodeDepthRev { get; set; }

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

    public string ToStringFull() => $"{Description}, Depth:{NodeDepthFwd}+{NodeDepthRev}={NodeDepth} => {ToString()}";

    public string ToStringSummary() => $"{Description}(Depth:{NodeDepthFwd}+{NodeDepthRev}={NodeDepth}, Steps:{Count})";

}
