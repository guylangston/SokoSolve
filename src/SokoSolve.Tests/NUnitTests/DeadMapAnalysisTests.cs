using NUnit.Framework;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Game;
using SokoSolve.Core.Library;

namespace SokoSolve.Tests.NUnitTests
{
    [TestFixture]
    public class DeadMapAnalysisTests
    {
        [Test]
        public void Box()
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

            var dead = DeadMapAnalysis.FindDeadMap(stat);
            Assert.NotNull(dead);
            report.WriteLine(dead);

            Assert.AreEqual(new TestReport(
                @".....
.XXX.
.X.X.
.XXX.
....."), report);
        }

        [Test]
        public void DeadMap()
        {
            // Init
            var report = new TestReport();
            var stat   = StaticAnalysis.Generate(TestLibrary.Default.Puzzle);
            var dead   = DeadMapAnalysis.FindDeadMap(stat);

            Assert.NotNull(dead);
            report.WriteLine(dead);

//            Assert.That(report, Is.EqualTo(new TestReport(
//@"...........
//...........
//...........
//.......X...
//...........
//...........
//.......X...
//...X..X....
//...........
//...........
//..........."
//            )));
            Assert.Inconclusive();
        }

        [Test]
        public void DynamicDeadMap()
        {
            // Init
            var p = Puzzle.Builder.FromLines(
                new[]
                {
                    "##########",
                    "#...XX...#",
                    "#...X#...#",
                    "#.P......#",
                    "##########"
                });
            var stat = StaticAnalysis.Generate(p);

            Assert.True(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));
            

            p = Puzzle.Builder.FromLines(
                new[]
                {
                    "##########",
                    "#...#X...#",
                    "#...X#...#",
                    "#.P......#",
                    "##########"
                });
            stat = StaticAnalysis.Generate(p);
            Assert.True(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));

            p = Puzzle.Builder.FromLines(
                new[]
                {
                    "##########",
                    "#...##...#",
                    "#...XX...#",
                    "#.P......#",
                    "##########"
                });
            stat = StaticAnalysis.Generate(p);
            Assert.True(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));

            p = Puzzle.Builder.FromLines(
                new[]
                {
                    "##########",
                    "#........#",
                    "#...XX...#",
                    "#...##...#",
                    "#.P......#",
                    "##########"
                });
            stat = StaticAnalysis.Generate(p);
            Assert.True(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));

            p = Puzzle.Builder.FromLines(
                new[]
                {
                    "##########",
                    "#........#",
                    "#...XX...#",
                    "#...X#...#",
                    "#.P......#",
                    "##########"
                });
            stat = StaticAnalysis.Generate(p);
            Assert.True(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));

            p = Puzzle.Builder.FromLines(
                new[]
                {
                    "##########",
                    "#........#",
                    "#...$$...#",
                    "#...$#...#",
                    "#.P......#",
                    "##########"
                });
            stat = StaticAnalysis.Generate(p);
            Assert.True(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));

            p = Puzzle.Builder.FromLines(
                new[]
                {
                    "##########",
                    "#........#",
                    "#...$$...#",
                    "#...X#...#",
                    "#.P......#",
                    "##########"
                });
            stat = StaticAnalysis.Generate(p);
            Assert.True(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));

            p = Puzzle.Builder.FromLines(
                new[]
                {
                    "##########",
                    "#........#",
                    "#...$$...#",
                    "#...#X...#",
                    "#.P......#",
                    "##########"
                });
            stat = StaticAnalysis.Generate(p);
            Assert.True(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));

            p = Puzzle.Builder.FromLines(
                new[]
                {
                    "##########",
                    "#........#",
                    "#...#X...#",
                    "#...$$...#",
                    "#.P......#",
                    "##########"
                });
            stat = StaticAnalysis.Generate(p);
            Assert.True(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));
        }

        [Test]
        public void Regression2()
        {
            // Init
            var report = new TestReport();

            var puz = Puzzle.Builder.FromLines(new[]
            {
                "################",
                "#..............#",
                "#..............#",
                "#.############.#",
                "#..............#",
                "################"
            });

            var stat = StaticAnalysis.Generate(puz);
            var dead = DeadMapAnalysis.FindDeadMap(stat);
            Assert.NotNull(dead);
            report.WriteLine(dead);

            //            Assert.That(report, Is.EqualTo(new TestReport(
            //@"...........
            //...........
            //...........
            //.......X...
            //...........
            //...........
            //.......X...
            //...X..X....
            //...........
            //...........
            //..........."
            //        
        }
    }
}