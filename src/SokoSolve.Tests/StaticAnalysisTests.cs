﻿using NUnit.Framework;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Game;
using SokoSolve.Core.Library;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Tests
{
    [TestFixture]
    public class StaticAnalysisTests
    {
        [Test]
        public void CornerMap()
        {
            // Init
            var report = new TestReport();

            var stat = StaticAnalysis.Generate(Puzzle.Builder.DefaultTestPuzzle());

            Assert.That(stat.CornerMap, Is.Not.Null);
            report.WriteLine(stat.CornerMap);

            Assert.That(report, Is.EqualTo(new TestReport(
                @"...........
....X......
...X....XX.
..X........
.X....X....
...........
....X.X..X.
........X..
..X....X...
..X...X....
..........."
            )));
        }

        [Test]
        public void DoorMap()
        {
            // Init
            var report = new TestReport();

            var stat = StaticAnalysis.Generate(Puzzle.Builder.DefaultTestPuzzle());

            Assert.That(stat.DoorMap, Is.Not.Null);
            report.WriteLine(stat.DoorMap);

            Assert.That(report, Is.EqualTo(new TestReport(
@"...........
...........
...........
.......X...
...........
...........
.......X...
...X..X....
...........
...........
..........."
            )));
        }


        [Test]
        public void Normalise()
        {
            var report = new TestReport();
            var p = Puzzle.Builder.DefaultTestPuzzle();

            Assert.IsTrue(p.Definition.Wall.Equals(p[0, 0]));
            Assert.IsTrue(p.Definition.Void == p[1, 1]);
            Assert.IsTrue(p.Definition.Player == p[4, 4]);
            Assert.IsTrue(p[4, 4].IsPlayer);
            
            Assert.AreEqual(new VectorInt2(4, 4), p.Player.Position);
            
            var norm = StaticAnalysis.Normalise(p);

            report.WriteLine(norm.ToString());
            Assert.That(report, Is.EqualTo(new TestReport(
                @"###########
####.######
###..###..#
##.X......#
#...PX.#..#
###.X###..#
###..#OO..#
###.##O#.##
##......###
##.....####
###########
"
            )));
        }

        [Test]
        public void Recesses()
        {
            // Init
            var report = new TestReport();

            var stat = StaticAnalysis.Generate(Puzzle.Builder.DefaultTestPuzzle());

            Assert.That(stat.RecessMap, Is.Not.Null);
            foreach (var recess in stat.RecessMap) report.WriteLine(recess);

            Assert.That(report, Is.EqualTo(new TestReport(
                @"(8,2) => (9,2)
...........
...........
........XX.
...........
...........
...........
...........
...........
...........
...........
...........

(2,9) => (6,9)
...........
...........
...........
...........
...........
...........
...........
...........
...........
..XXXXX....
...........

(2,8) => (2,9)
...........
...........
...........
...........
...........
...........
...........
...........
..X........
..X........
...........

(9,2) => (9,6)
...........
...........
.........X.
.........X.
.........X.
.........X.
.........X.
...........
...........
...........
..........."
            )));
        }


        [Test]
        public void SideMap()
        {
            // Init
            var report = new TestReport();

            var stat = StaticAnalysis.Generate(Puzzle.Builder.DefaultTestPuzzle());

            Assert.That(stat.SideMap, Is.Not.Null);
            report.WriteLine(stat.SideMap);

            Assert.That(report, Is.EqualTo(new TestReport(
                @"...........
...........
...........
......X..X.
.........X.
.........X.
...X.......
...........
...........
...XXX.....
..........."
            )));
        }


        [Test]
        public void Walls()
        {
            // Init
            var report = new TestReport();

            var stat = StaticAnalysis.Generate(Puzzle.Builder.DefaultTestPuzzle());

            Assert.That(stat.IndividualWalls, Is.Not.Null);
            foreach (var wall in stat.IndividualWalls) report.WriteLine(wall);

            Assert.That(report, Is.EqualTo(new TestReport(
                @"(8,2) => (9,2)
...........
...........
........XX.
...........
...........
...........
...........
...........
...........
...........
...........

(3,6) => (4,6)
...........
...........
...........
...........
...........
...........
...XX......
...........
...........
...........
...........

(2,9) => (6,9)
...........
...........
...........
...........
...........
...........
...........
...........
...........
..XXXXX....
...........

(2,8) => (2,9)
...........
...........
...........
...........
...........
...........
...........
...........
..X........
..X........
...........

(6,3) => (6,4)
...........
...........
...........
......X....
......X....
...........
...........
...........
...........
...........
...........

(9,2) => (9,6)
...........
...........
.........X.
.........X.
.........X.
.........X.
.........X.
...........
...........
...........
...........
"
            )));
        }

        [Test]
        public void Walls_Box_3v3()
        {
            // Init
            var report = new TestReport();

            var puz = Puzzle.Builder.FromLines(new[]
            {
                "#####",
                "#...#",
                "#...#",
                "#...#",
                "#####"
            });


            var stat = StaticAnalysis.Generate(puz);

            Assert.That(stat.IndividualWalls, Is.Not.Null);


            foreach (var wall in stat.IndividualWalls) report.WriteLine(wall);

            Assert.That(report, Is.EqualTo(new TestReport(
                @"(1,1) => (3,1)
.....
.XXX.
.....
.....
.....

(1,3) => (3,3)
.....
.....
.....
.XXX.
.....

(1,1) => (1,3)
.....
.X...
.X...
.X...
.....

(3,1) => (3,3)
.....
...X.
...X.
...X.
....."
            )));
        }


        [Test]
        public void Walls_Box_4v4()
        {
            // Init
            var report = new TestReport();

            var puz = Puzzle.Builder.FromLines(new[]
            {
                "######",
                "#....#",
                "#....#",
                "#....#",
                "#....#",
                "######"
            });


            var stat = StaticAnalysis.Generate(puz);

            Assert.That(stat.IndividualWalls, Is.Not.Null);


            foreach (var wall in stat.IndividualWalls) report.WriteLine(wall);

            Assert.That(report, Is.EqualTo(new TestReport(
                @"(1,1) => (4,1)
......
.XXXX.
......
......
......
......

(1,4) => (4,4)
......
......
......
......
.XXXX.
......

(1,1) => (1,4)
......
.X....
.X....
.X....
.X....
......

(4,1) => (4,4)
......
....X.
....X.
....X.
....X.
......
"
            )));
        }
    }
}