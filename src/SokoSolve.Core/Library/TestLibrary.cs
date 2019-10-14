using System.Collections.Generic;
using SokoSolve.Core.Analytics;

namespace SokoSolve.Core.Library
{
    public class TestLibrary
    {
        public static LibraryPuzzle Default = new LibraryPuzzle(new Puzzle.Puzzle(new List<string>
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
        }))
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


        public static LibraryPuzzle SlimyTown = new LibraryPuzzle(new Puzzle.Puzzle(new[]
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


        public static LibraryPuzzle GrimTown = new LibraryPuzzle(new Puzzle.Puzzle(new[]
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