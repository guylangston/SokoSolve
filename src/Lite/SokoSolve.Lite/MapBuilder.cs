using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SokoSolve.Lite
{
    public class MapBuilder // Create maps from strings, etc
    {
        public static Map FromLines(IReadOnlyList<string> map, Definition def)
        {
            var cells = new Cell[map.Max(x => x.Length), map.Count];

            for (int y = 0; y < map.Count; y++)
            {
                for (int x = 0; x < map[y].Length; x++)
                {
                    cells[y, x] = def.ToCell(map[y][x]);
                }
            }

            return new Map(cells);
        }

        public static readonly Map  Default = FromLines(new List<string>
        {
            "#~~###~~~~#",
            "~~##.#~####",
            "~##..###..#",
            "##.X......#",
            "#...PX.#..#",
            "###.X###..#",
            "~~#..#OO..#",
            "~##.##O#.##",
            "~#......##~",
            "~#.....##~~",
            "########~~~"
        }, Definition.Default);

        public static readonly string DefaultSolution = "UULDDDDDUUUURRRRRDDDLLDDDLLLLURRLUURUULURRRRLLLDDDLDDDRRRUUURRUURUULDDULLDLULLDDRULURRRRLDLLDDLDDDRRRUUURRRUULDURUULDLLDLLDDLDDRRDRULLLUUUUURRRRRRDDDLLRURUULDDRDL";
    }
}
