using System;
using System.Collections.Generic;

namespace SokoSolve.Lite
{
    public static class Util
    {
        public static IEnumerable<VectorInt2> ToPath(string p)
        {
            foreach (var chr in p)
            {
                yield return chr switch
                {
                    'L' => VectorInt2.Left,
                    'R' => VectorInt2.Right,
                    'U' => VectorInt2.Up,
                    'D' => VectorInt2.Down,
                    _   => throw new InvalidCastException(chr.ToString())
                };
            }
        }

        public static IEnumerable<Block> ToBlocks(Cell c)
        {
            var cc = (int) c;
            if ((cc & (int) Block.Void) > 0) yield return Block.Void;
            if ((cc & (int) Block.Wall) > 0) yield return Block.Wall;
            if ((cc & (int) Block.Floor) > 0) yield return Block.Floor;
            if ((cc & (int) Block.Goal) > 0) yield return Block.Goal;
            if ((cc & (int) Block.Crate) > 0) yield return Block.Crate;
            if ((cc & (int) Block.Player) > 0) yield return Block.Player;
        }
    }
}
