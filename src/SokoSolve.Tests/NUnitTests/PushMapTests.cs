using System.Linq;
using NUnit.Framework;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Game;
using SokoSolve.Core.Primitives;
using SokoSolve.Tests.NUnitTests;
using VectorInt;

namespace SokoSolve.Tests
{
    [TestFixture]
    public class PushMapTests
    {
        [Test]
        public void CaseA()
        {
            var report = new TestReport();

            var sample = new[]
            {
                "#############",
                "#a ###    b##",
                "# A######  ##",
                "#  ######B ##",
                "#########  ##",
                "#############"
            };


            var boundry = Bitmap.Create(sample, x => x == '#' ); //sample.ToMap('#', 'A', 'B');

            var staticMaps = new StaticMaps
            {
                FloorMap = Bitmap.Create(sample,' ', 'a', 'b'),
                WallMap = Bitmap.Create(sample,'#')
            };
            var stateMaps = new StateMaps
            {
                CrateMap = Bitmap.Create(sample,'A', 'B'),
                MoveMap = FloodFill.Fill(staticMaps.WallMap, Bitmap.FindPosition(sample, 'a'))
            };

            var pushMap = PushMap.Find(staticMaps, stateMaps, Bitmap.FindPosition(sample, 'A'),
                Bitmap.FindPosition(sample, 'b'));


            report.WriteLine(pushMap);

            Assert.That(report, Is.EqualTo(new TestReport(
                @".............
..X..........
.............
..X..........
.............
............."
            )));
        }

        [Test]
        public void CaseB()
        {
            var report = new TestReport();

            var sample = Puzzle.Builder.FromLines(new[]
            {
                "#############",
                "#a ###    b##",
                "# A######  ##",
                "#  ######B ##",
                "#########  ##",
                "#############"
            });

            var staticMaps = new StaticMaps
            {
                FloorMap = sample.ToMap(' ', 'a', 'b'),
                WallMap = sample.ToMap('#')
            };
            var stateMaps = new StateMaps
            {
                CrateMap = sample.ToMap('A', 'B'),
                MoveMap = FloodFill.Fill(staticMaps.WallMap, sample.First(x => x.Value.Underlying == 'b').Position)
            };

            var pushMap = PushMap.Find(staticMaps, stateMaps, sample.First(x => x.Value.Underlying == 'B').Position,
                sample.First(x => x.Value.Underlying == 'b').Position);


            report.WriteLine(pushMap);

            Assert.That(report, Is.EqualTo(new TestReport(
                @".............
......XXXX...
.........X...
.............
.........X...
............."
            )));
        }

        [Test]
        public void CaseB_WithPath()
        {
            var report = new TestReport();

            var sample = Puzzle.Builder.FromLines(new[]
            {
                "#############",
                "#a ###    b##",
                "# A######  ##",
                "#  ######B ##",
                "#########  ##",
                "#############"
            });

            var staticMaps = new StaticMaps
            {
                FloorMap = sample.ToMap(' ', 'a', 'b'),
                WallMap = sample.ToMap('#')
            };
            var stateMaps = new StateMaps
            {
                CrateMap = sample.ToMap('A', 'B'),
                MoveMap = FloodFill.Fill(staticMaps.WallMap, sample.First(x => x.Value.Underlying == 'b').Position)
            };

            var pushMap = PushMap.Find(staticMaps, stateMaps, sample.First(x => x.Value.Underlying == 'B').Position,
                sample.First(x => x.Value.Underlying == 'b').Position);


            report.WriteLine(pushMap);


            var path = pushMap.FindPlayerWalkRoute(new VectorInt2(6, 1));
            report.WriteLine(path);

            Assert.That(report, Is.EqualTo(new TestReport(
                @".............
......XXXX...
.........X...
.............
.........X...
.............

DDDLUURULLL
")));
        }

        [Test]
        public void MultipleCrates()
        {
            var report = new TestReport();

            var sample = Puzzle.Builder.FromLines(new[]
            {
                "#############",
                "#   x     p##",
                "#x         ##",
                "#  ######X ##",
                "#########  ##",
                "#############"
            });


            var staticMaps = new StaticMaps
            {
                FloorMap = sample.ToMap(' ', 'x', 'X', 'p'),
                WallMap = sample.ToMap('#')
            };

            var crate = sample.ToMap('X', 'x');
            var stateMaps = new StateMaps
            {
                CrateMap = crate,
                MoveMap = FloodFill.Fill(staticMaps.WallMap.BitwiseOR(crate),
                    sample.First(x => x.Value.Underlying == 'p').Position)
            };

            var pushMap = PushMap.Find(staticMaps, stateMaps, sample.First(x => x.Value.Underlying == 'X').Position,
                sample.First(x => x.Value.Underlying == 'p').Position);


            report.WriteLine(pushMap);

            Assert.That(report, Is.EqualTo(new TestReport(
                @".............
.....XXXXXX..
..XXXXXXXXX..
..X......XX..
.........XX..
.............
"
            )));
        }

        [Test]
        public void OverAndBackAgain()
        {
            var report = new TestReport();

            var sample = Puzzle.Builder.FromLines(new[]
            {
                "#############",
                "#...........#",
                "#...........#",
                "#...........#",
                "######.######",
                "#......x....#",
                "#o.........p#",
                "#############"
            });


            var staticMaps = new StaticMaps
            {
                FloorMap = sample.ToMap('.', 'x', 'o', 'p'),
                WallMap = sample.ToMap('#')
            };

            var crate = sample.ToMap('X', 'x');
            var stateMaps = new StateMaps
            {
                CrateMap = crate,
                MoveMap = FloodFill.Fill(staticMaps.WallMap.BitwiseOR(crate),
                    sample.First(x => x.Value.Underlying == 'p').Position)
            };

            var from = sample.First(x => x.Value.Underlying == 'x').Position;
            var to = sample.First(x => x.Value.Underlying == 'p').Position;
            var pushMap = PushMap.Find(staticMaps, stateMaps, from, to);
            report.WriteLine(pushMap);

            Assert.That(report, Is.EqualTo(new TestReport(
                @".............
.XXXXXXXXXXX.
.XXXXXXXXXXX.
.XXXXXXXXXXX.
......X......
.XXXXXXXXXXX.
.XXXXXXXXXXX.
.............
"
            )));


            var playerRoute = pushMap.FindPlayerWalkRoute(to);
            report.WriteLine("pushMap.FindPlayerWalkRoute(to)");
            report.WriteLine(playerRoute);


            var crateRoute = pushMap.FindCrateRoute(to);
            report.WriteLine("pushMap.FindCrateRoute(to)");
            report.WriteLine(crateRoute);
        }


        [Test]
        public void Regression1()
        {
            var report = new TestReport();
            var defaultPuzzle = Puzzle.Builder.DefaultTestPuzzle(); // default puzzle
            var analysis = new PuzzleAnalysis(defaultPuzzle);
            var state = analysis.Evalute(defaultPuzzle);

            var pushMap = PushMap.Find(state, new VectorInt2(3, 3), defaultPuzzle.Player.Position);


            report.WriteLine("===================");
            report.WriteLine(defaultPuzzle);
            report.WriteLine(pushMap);
            report.WriteLine("===================");

            var r = pushMap.FindPlayerWalkRoute(new VectorInt2(7, 3));
            report.WriteLine(r);

            Assert.That(report, Is.EqualTo(new TestReport(
                @"===================
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

...........
....X......
...XX...XX.
..XXXXXXXX.
.XXXX...XX.
...X....XX.
...X..XXXX.
...X..X.X..
..XXXXXX...
..XXXXX....
...........

===================
LLURRRR
"
            )));
        }
    }
}