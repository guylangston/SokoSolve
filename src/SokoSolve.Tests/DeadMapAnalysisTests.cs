using NUnit.Framework;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Library;
using SokoSolve.Core.Puzzle;

namespace SokoSolve.Tests
{
    [TestFixture]
    public class DeadMapAnalysisTests
    {
        [Test]
        public void Box()
        {
            // Init
            var report = new TestReport();

            var puz = new Puzzle(new[]
            {
                "#####",
                "#...#",
                "#...#",
                "#...#",
                "#####"
            });

            var stat = StaticAnalysis.Generate(puz);

            var dead = DeadMapAnalysis.FindDeadMap(stat);
            Assert.That(dead, Is.Not.Null);
            report.WriteLine(dead);

            Assert.That(report, Is.EqualTo(new TestReport(
                @".....
.XXX.
.X.X.
.XXX.
....."
            )));
        }

        [Test]
        public void DeadMap()
        {
            // Init
            var report = new TestReport();

            var stat = StaticAnalysis.Generate(TestLibrary.Default);

            var dead = DeadMapAnalysis.FindDeadMap(stat);


            Assert.That(dead, Is.Not.Null);
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
            var report = new TestReport();

            var p = new Puzzle(
                new[]
                {
                    "##########",
                    "#...XX...#",
                    "#...X#...#",
                    "#.P......#",
                    "##########"
                });
            var stat = StaticAnalysis.Generate(p);

            Assert.That(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)), Is.True);

            p = new Puzzle(
                new[]
                {
                    "##########",
                    "#...#X...#",
                    "#...X#...#",
                    "#.P......#",
                    "##########"
                });
            stat = StaticAnalysis.Generate(p);
            Assert.That(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)), Is.True);

            p = new Puzzle(
                new[]
                {
                    "##########",
                    "#...##...#",
                    "#...XX...#",
                    "#.P......#",
                    "##########"
                });
            stat = StaticAnalysis.Generate(p);
            Assert.That(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)), Is.True);

            p = new Puzzle(
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
            Assert.That(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)), Is.True);

            p = new Puzzle(
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
            Assert.That(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)), Is.True);

            p = new Puzzle(
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
            Assert.That(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)), Is.False);

            p = new Puzzle(
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
            Assert.That(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)), Is.True);


            p = new Puzzle(
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
            Assert.That(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)), Is.True);

            p = new Puzzle(
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
            Assert.That(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)), Is.True);
        }

        [Test]
        public void Regression2()
        {
            // Init
            var report = new TestReport();

            var puz = new Puzzle(new[]
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
            Assert.That(dead, Is.Not.Null);
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