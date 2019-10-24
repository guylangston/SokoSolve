using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Game;
using Xunit;

namespace SokoSolve.Tests.NUnitTests
{
    public class BitStream : Queue<bool>
    {
        public static int Max = Convert.ToInt32("0111", 2);

        public BitStream()
        {
        }

        public BitStream(IEnumerable<bool> collection) : base(collection)
        {
        }

        public BitStream(params bool[] args) : base(args)
        {
        }

        public BitStream(params int[] args) : base(args.Select(x => x == 1))
        {
        }

        public static BitStream Encode(Control c)
        {
            switch (c)
            {
                case Control.None: return new BitStream(0, 0, 0);
                case Control.Floor: return new BitStream(0, 0, 1);
                case Control.Crate: return new BitStream(0, 1, 0);
                case Control.Goal: return new BitStream(0, 1, 1);
                case Control.CrateGoal: return new BitStream(1, 0, 0);
                case Control.Player: return new BitStream(1, 0, 1);
                case Control.PlayerGoal: return new BitStream(1, 1, 0);
                case Control.End: return new BitStream(1, 1, 1);
            }

            throw new Exception();
        }

        public void AddRange(BitStream encode)
        {
            foreach (var bit in encode) Enqueue(bit);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var len = 0;
            foreach (var item in this)
            {
                sb.Append(item ? "1" : "0");
                if (++len % 32 == 0) sb.AppendLine();
            }

            return sb.ToString();
        }

        public static BitStream Encode(int num)
        {
            if (num > Max) throw new Exception();
            return new BitStream(
                (num & (1 << 2)) > 0,
                (num & (1 << 1)) > 0,
                (num & 1) > 0
            );
        }

        public byte[] ToByteArray()
        {
            var res = new List<byte>();
            byte run = 0;
            byte curr = 0;
            foreach (var item in this)
            {
                curr += (byte) ((item ? 1 : 0) << run);
                if (++run == 8)
                {
                    run = 0;
                    res.Add(curr);
                    curr = 0;
                }
            }

            return res.ToArray();
        }
    }

    public enum Control
    {
        None,
        Floor,
        Crate,
        Goal,
        CrateGoal,
        Player,
        PlayerGoal,
        End
    }

    public static class RunLength
    {
        public static List<Tuple<T, int>> Encode<T>(IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            var r = new List<Tuple<T, int>>();

            var e = source.GetEnumerator();
            e.MoveNext();

            var prev = e.Current;
            var run = 1;
            while (e.MoveNext())
                if (comparer.Equals(prev, e.Current))
                {
                    run++;
                }
                else
                {
                    r.Add(new Tuple<T, int>(prev, run));
                    run = 1;
                    prev = e.Current;
                }

            return r;
        }
    }

    public class BitStreamRunLengthEncoding : IEqualityComparer<Puzzle.Tile>
    {
        private BitStream Encode(Puzzle puzzle)
        {
            var norm = StaticAnalysis.RemoveOuter(StaticAnalysis.Normalise(puzzle));
            var bs = new BitStream();
            foreach (var cell in RunLength.Encode(norm.ForEachTile(), this))
            {
                var s = cell.Item1.Cell;
                if (s == puzzle.Definition.Void ||
                    s == puzzle.Definition.Wall)
                    EncodeItem(cell, Control.None, bs);

                if (s == puzzle.Definition.Floor) EncodeItem(cell, Control.Floor, bs);

                if (s == puzzle.Definition.Goal) bs.AddRange(BitStream.Encode(Control.Goal));
                if (s == puzzle.Definition.Crate) bs.AddRange(BitStream.Encode(Control.Crate));
                if (s == puzzle.Definition.CrateGoal) bs.AddRange(BitStream.Encode(Control.CrateGoal));
                if (s == puzzle.Definition.Player) bs.AddRange(BitStream.Encode(Control.Player));
                if (s == puzzle.Definition.PlayerGoal) bs.AddRange(BitStream.Encode(Control.PlayerGoal));
            }

            bs.AddRange(BitStream.Encode(Control.End));
            return bs;
        }

        private static void EncodeItem(Tuple<Puzzle.Tile, int> cell, Control state, BitStream bs)
        {
            if (cell.Item2 > BitStream.Max)
            {
                for (var cc = 0; cc < cell.Item2 / BitStream.Max; cc++)
                {
                    bs.AddRange(BitStream.Encode(state));
                    bs.AddRange(BitStream.Encode(BitStream.Max));
                }

                var rem = cell.Item2 % BitStream.Max;
                if (rem > 0)
                {
                    bs.AddRange(BitStream.Encode(state));
                    bs.AddRange(BitStream.Encode(rem));
                }
            }
            else
            {
                bs.AddRange(BitStream.Encode(state));
                bs.AddRange(BitStream.Encode(cell.Item2));
            }
        }

        public bool Equals(Puzzle.Tile x, Puzzle.Tile y)
        {
            return x.Cell == y.Cell;
        }

        public int GetHashCode(Puzzle.Tile obj)
        {
            throw new NotImplementedException();
        }

        [Xunit.Fact]
        public void EncodePuzzle()
        {
            var p = Puzzle.Builder.DefaultTestPuzzle();
            var stream = Encode(p);
            Console.WriteLine(stream.ToString());
        }

        [Xunit.Fact]
        public void EncodePuzzleToBase64()
        {
            var p = Puzzle.Builder.DefaultTestPuzzle();
            var stream = Encode(p);
            var bytes = stream.ToByteArray();
            Console.WriteLine(Convert.ToBase64String(bytes));
        }
    }
}