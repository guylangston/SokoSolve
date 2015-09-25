using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Sokoban.Core.Analytics;
using Sokoban.Core.PuzzleLogic;

namespace Sokoban.Core.Library
{

    public class TestLibrary
    {

        public static LibraryPuzzle Default = new LibraryPuzzle(new Puzzle(new List<string>
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
            "########~~~#",
        }))
        {
            Details = new AuthoredItem()
            {
                
                Author = "David W. Skinner",
                Email = "sasquatch@bentonrea.com",
                Url = "http://users.bentonrea.com/~sasquatch/",    
            },
            
            Solution = new Path("llurrrrrllldlddddrrruurruuruulddulldlluldddduuuurrrrrdrddllldddllllurrluuruulurrrrldllddldddrrruuurrruulduruuldlldllddlddrrdrullluuuuurrrrrrdddllruruulddrdl")
        };


        public static LibraryPuzzle SlimyTown = new LibraryPuzzle(new Puzzle(new[]
        {
          "~##~#####",
          "##.##.O.#",
          "#.##.XO.#",
          "~##.X...#",
          "##.XP.###",
          "#.X..##~~",
          "#OO.##.##",
          "#...#~##~",
          "#####~#~~",
        }))
        {
            Details = new AuthoredItem()
            {
                Name = "Slimy Grave"
            },
            Solution = new Path("ruruulrddldldldlluRuRuRuRurrddlUlldRlldDuurrdLulDrdLulDlddrrULuurrdLulDurururrrdLullddrUluR")
        };

      

        public static LibraryPuzzle GrimTown = new LibraryPuzzle(new Puzzle(new []
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
          "~####~~~~~~~~~~~",
        }))
        {
            Details = new AuthoredItem()
            {
                Name = "Grim Town",
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
