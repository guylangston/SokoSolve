using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NUnit.Framework;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.PuzzleLogic;

namespace SokoSolve.Tests
{

    [TestFixture]
    public class PushMapTests
    {
        [Test]
        public void CaseA()
        {
            var report = new TestReport();

            var sample = new Puzzle(new string[]
            {
                "#############",
                "#a ###    b##",
                "# A######  ##",
                "#  ######B ##",
                "#########  ##",
                "#############",
            });


            var boundry = sample.ToMap('#', 'A', 'B' );

            var staticMaps = new StaticMaps()
            {
                FloorMap = sample.ToMap(' ', 'a', 'b'),
                WallMap = sample.ToMap('#')
            };
            var stateMaps = new StateMaps()
            {
                CrateMap = sample.ToMap('A', 'B'),
                MoveMap = FloodFill.Fill(staticMaps.WallMap, sample.First(x => x.State == 'a').Position)
            };

            var pushMap = PushMap.Find(staticMaps, stateMaps, sample.First(x => x.State == 'A').Position, sample.First(x => x.State == 'a').Position);

            
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

            var sample = new Puzzle(new string[]
            {
                "#############",
                "#a ###    b##",
                "# A######  ##",
                "#  ######B ##",
                "#########  ##",
                "#############",
            });

            var staticMaps = new StaticMaps()
            {
                FloorMap = sample.ToMap(' ', 'a', 'b'),
                WallMap = sample.ToMap('#')
            };
            var stateMaps = new StateMaps()
            {
                CrateMap = sample.ToMap('A', 'B'),
                MoveMap = FloodFill.Fill(staticMaps.WallMap, sample.First(x => x.State == 'b').Position)
            };

            var pushMap = PushMap.Find(staticMaps, stateMaps, sample.First(x => x.State == 'B').Position, sample.First(x => x.State == 'b').Position);


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

            var sample = new Puzzle(new string[]
            {
                "#############",
                "#a ###    b##",
                "# A######  ##",
                "#  ######B ##",
                "#########  ##",
                "#############",
            });

            var staticMaps = new StaticMaps()
            {
                FloorMap = sample.ToMap(' ', 'a', 'b'),
                WallMap = sample.ToMap('#')
            };
            var stateMaps = new StateMaps()
            {
                CrateMap = sample.ToMap('A', 'B'),
                MoveMap = FloodFill.Fill(staticMaps.WallMap, sample.First(x => x.State == 'b').Position)
            };

            var pushMap = PushMap.Find(staticMaps, stateMaps, sample.First(x => x.State == 'B').Position, sample.First(x => x.State == 'b').Position);


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

            var sample = new Puzzle(new string[]
            {
                "#############",
                "#   x     p##",
                "#x         ##",
                "#  ######X ##",
                "#########  ##",
                "#############",
            });


            var staticMaps = new StaticMaps()
            {
                FloorMap = sample.ToMap(' ', 'x', 'X', 'p'),
                WallMap = sample.ToMap('#')
            };

            var crate = sample.ToMap('X', 'x');
            var stateMaps = new StateMaps()
            {
                CrateMap = crate,
                MoveMap = FloodFill.Fill(staticMaps.WallMap.BitwiseOR(crate), sample.First(x => x.State == 'p').Position)
            };

            var pushMap = PushMap.Find(staticMaps, stateMaps, sample.First(x => x.State == 'X').Position, sample.First(x => x.State == 'p').Position);


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

            var sample = new Puzzle(new string[]
            {
                "#############",
                "#...........#",
                "#...........#",
                "#...........#",
                "######.######",
                "#......x....#",
                "#o.........p#",
                "#############",
            });


            var staticMaps = new StaticMaps()
            {
                FloorMap = sample.ToMap('.', 'x','o', 'p'),
                WallMap = sample.ToMap('#')
            };

            var crate = sample.ToMap('X', 'x');
            var stateMaps = new StateMaps()
            {
                CrateMap = crate,
                MoveMap = FloodFill.Fill(staticMaps.WallMap.BitwiseOR(crate), sample.First(x => x.State == 'p').Position)
            };

            var from = sample.First(x => x.State == 'x').Position;
            var to = sample.First(x => x.State == 'p').Position;
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
            var defaultPuzzle = new Puzzle(); // default puzzle
            var analysis = new PuzzleAnalysis(defaultPuzzle);
            var state = analysis.Evalute(defaultPuzzle);

            var pushMap = PushMap.Find(state, new VectorInt2(3,3), defaultPuzzle.Player.Position);


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
