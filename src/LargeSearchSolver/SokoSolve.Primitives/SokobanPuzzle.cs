using System.Text;
using VectorInt;
using VectorInt.Collections;

namespace SokoSolve.Primitives;

public class SokobanPuzzle : Puzzle<char>
{
    private SokobanPuzzle(IReadOnlyCartesianMap<CellDefinition<char>> map, CellDefinition<char>.Set definition) : base(map, definition)
    {
    }

    public SokobanPuzzle Clone() => new SokobanPuzzle(this, Definition);

    public static class Builder
    {
        public static SokobanPuzzle FromLines(IEnumerable<string> puzzleStr, CharCellDefinition.Set? defn = null)
        {
            defn ??= CharCellDefinition.Default;
            return new SokobanPuzzle(
                CartesianMapBuilder.Create(puzzleStr.Select(line => line.Select(c=>defn.Get(c)).ToList()).ToList()),
                CharCellDefinition.Default);
        }

        public static SokobanPuzzle FromMultLine(string puzzleText)
            => FromLines(
                puzzleText.Split('\n').Select(x => x.Trim('\r')).Where(x => x.Length > 0),
                CharCellDefinition.Default);

        public static SokobanPuzzle CreateEmpty()
        {
            return FromLines(
            [
                "####",
                "#..#",
                "#..#",
                "####",
            ]);
        }
    }

    public new List<string> ToStringList()
    {
        var res = new List<string>();
        for (var y = 0; y < Height; y++)
        {
            var sb = new StringBuilder();
            for (var x = 0; x < Width; x++) sb.Append(this[x, y]);
            res.Add(sb.ToString());
        }

        return res;
    }
}
