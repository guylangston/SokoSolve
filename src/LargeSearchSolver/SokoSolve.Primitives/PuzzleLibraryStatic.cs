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
}

