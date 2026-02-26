using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Lite.Tests
{
    public class GameLogicTests
    {
        private ITestOutputHelper outp;

        public GameLogicTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void CanInitGame()
        {
            var map = MapBuilder.Default;
            var gm  = new Game(map);
            gm.Init();

            Assert.Equal(MoveResult.Ok, gm.Move(VectorInt2.Right));
            Assert.Equal(MoveResult.Invalid, gm.Move(VectorInt2.Right));
        }

        [Fact]
        public void CanUndo()
        {
            var map = MapBuilder.Default;
            var gm  = new Game(map);
            gm.Init();

            Assert.Equal(MoveResult.Ok, gm.Move(VectorInt2.Right));
            Assert.Equal(0, gm.Statistics.Undos);
            Assert.Equal(1, gm.Statistics.Pushes);

            outp.WriteLine(gm.Current.ToString());
            Assert.Equal(@"#~~###~~~~#
~~##.#~####
~##..###..#
##.X......#
#....PX#..#
###.X###..#
~~#..#OO..#
~##.##O#.##
~#......##~
~#.....##~~
########~~~", gm.Current.ToString());

            gm.UndoMove();
            Assert.Equal(gm.Start.ToString(), gm.Current.ToString());
            Assert.Equal(1, gm.Statistics.Undos);

        }

        [Fact]
        public void CheckSolution()
        {
            var map = MapBuilder.Default;
            var gm  = new Game(map);
            gm.Init();

            var path = MapBuilder.DefaultSolution;
            var p    = Util.ToPath(path).ToArray();

            foreach (var move in p[..^1])
            {
                var r = gm.Move(move);
                Assert.Equal(MoveResult.Ok, r);
            }
            Assert.Equal(MoveResult.Win, gm.Move(p[^1]));

        }

    }
}
