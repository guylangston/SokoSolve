namespace SokoSolve.Primitives;

public static class PuzzleLibraryStatic
{
    public static readonly Puzzle PQ1_P1 = Puzzle.Builder.FromMultLine(
        """
        #~~###~~~~#
        ~~##.#~####
        ~##..###..#
        ##.X......#
        #...PX.#..#
        ###.X###..#
        ~~#..#OO..#
        ~##.##O#.##
        ~#......##~
        ~#.....##~~
        ########~~~
        """);

    /// <summary> 900k nodes </summary>
    public static readonly Puzzle PQ1_P29 = Puzzle.Builder.FromMultLine(
"""
~~~####~~~~
~~~#P.#~~~~
~~##..##~~~
~~#.OX#####
~~#XO.#...#
###OOX#.#.#
#..OOX..X.#
#.X.X.#.###
#####.#.#~~
~~~~#...#~~
~~~~###O#~~
~~~~~~###~~
""");

    public static readonly Puzzle PQ1_P5 = Puzzle.Builder.FromMultLine(
"""
~~~~~~~~~~~#####
~~~~~~~~~~##...#
~~~~~~~~~~#....#
~~~~####~~#.X.##
~~~~#..####X.X#~
~~~~#.....X.X.#~
~~~##.##.X.X.X#~
~~~#..O#..X.X.#~
~~~#..O#......#~
#####.#########~
#OOOO.P..#~~~~~~
#OOOO....#~~~~~~
##..######~~~~~~
~####~~~~~~~~~~~
""");


    public static readonly Puzzle Trivial01 = Puzzle.Builder.FromMultLine(
"""
######
#PO..#
#..X.#
#.XO.#
#....#
######
""");

    public static readonly Puzzle Trivial02 = Puzzle.Builder.FromMultLine(
"""
######
#P...#
#.XX.#
#....#
#....#
#....#
#....#
#O..O#
######
""");

    public static readonly Puzzle Trivial03_NoSolution = Puzzle.Builder.FromMultLine(
"""
######
#PO..#
#..X.#
#.O..#
#X...#
######
""");
}

