using System.Collections.Generic;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Game;

namespace SokoSolve.Core.Library
{
    public class TestLibrary
    {
        public static List<string> DefaultPuzzleTest = new List<string>
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
            "########~~~#"
        };
        public static LibraryPuzzle Default = new LibraryPuzzle(Puzzle.Builder.FromLines(DefaultPuzzleTest))
        {
            Details = new AuthoredItem
            {
                Author = "David W. Skinner",
                Email = "sasquatch@bentonrea.com",
                Url = "http://users.bentonrea.com/~sasquatch/"
            },

            Solution = new Path(
                "llurrrrrllldlddddrrruurruuruulddulldlluldddduuuurrrrrdrddllldddllllurrluuruulurrrrldllddldddrrruuurrruulduruuldlldllddlddrrdrullluuuuurrrrrrdddllruruulddrdl")
        };


        public static LibraryPuzzle SlimyTown = new LibraryPuzzle(Puzzle.Builder.FromLines(new[]
        {
            "~##~#####",
            "##.##.O.#",
            "#.##.XO.#",
            "~##.X...#",
            "##.XP.###",
            "#.X..##~~",
            "#OO.##.##",
            "#...#~##~",
            "#####~#~~"
        }))
        {
            Details = new AuthoredItem
            {
                Name = "Slimy Grave"
            },
            Solution = new Path(
                "ruruulrddldldldlluRuRuRuRurrddlUlldRlldDuurrdLulDrdLulDlddrrULuurrdLulDurururrrdLullddrUluR")
        };


        public static LibraryPuzzle GrimTown = new LibraryPuzzle(Puzzle.Builder.FromLines(new[]
        {
            "~~~~~~~~~~~#####",
            "~~~~~~~~~~##...#",
            "~~~~~~~~~~#....#",
            "~~~~####~~#.X.##",
            "~~~~#..####X.X#~",
            "~~~~#.....X.X.#~",
            "~~~##.##.X.X.X#~",
            "~~~#..O#..X.X.#~",
            "~~~#..O#......#~",
            "#####.#########~",
            "#OOOO.P..#~~~~~~",
            "#OOOO....#~~~~~~",
            "##..######~~~~~~",
            "~####~~~~~~~~~~~"
        }))
        {
            Details = new AuthoredItem
            {
                Name = "Grim Town"
            },
            Solution = new Path(@"luuuuurrrdddrrruLuLrddlluUruLLLulDDDDDDrdLLullddrUluRRdrUUUl
                uRuurrrdddrUruLdlUruLLLulDDDldRuuurrrdddrrrrruLLLdlUruLdlUru
                LLLulDDDDDDrdLLullddrUruLLrrruuuuurrrddrrrruLLLdlUruLLLulDDD
                DDDrdLLullddrUluRRRRluuuuurrrdrrruLLLLLulDDDDDDrdLLLLurrruuu
                uurrrrrrUrDDlddrruLdlUUruLLLLLLulDDDDDDrdLLullddrUruruuuuurr
                rrrrurUruulDDDDDlddrruLdlUUruLLLLLLulDDDDDDrdLLLdlUrruruuuuu
                rrrrrruruulDDrdLLLLLLulDDDDDDrdLLLruruuuuurrrrrrrrUUluurrdLu
                lDDDrdLLLLLLLulDDDDDDrdLLuruuuuurrrddrrrrrUUUUluurrdLulDDDrd
                LLLLLLLulDDDDDDrdLrrruLLL")
        };
    }
}