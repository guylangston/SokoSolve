using SokoSolve.Core;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Lib;
using Xunit;

namespace SokoSolve.Tests.AnalysisTests
{
    public class DeadMapAnalysisTests
    {
        [Xunit.Fact]
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

            var stat = new StaticAnalysisMaps(puz);

            var dead = DeadMapAnalysis.FindDeadMap(stat);
            Assert.NotNull(dead);
            report.WriteLine(dead);

            Assert.Equal(new TestReport(
                @".....
.XXX.
.X.X.
.XXX.
....."), report);
        }

        [Xunit.Fact]
        public void DeadMap()
        {
            // Init
            var report = new TestReport();
            
            var stat = new StaticAnalysisMaps(TestLibrary.Default.Puzzle);
            var dead   = DeadMapAnalysis.FindDeadMap(stat);

            Assert.NotNull(dead);
            report.WriteLine(dead);
            
           
            var expect = new TestReport(
@"...........
....X......
...X....XX.
..X......X.
.X....X..X.
.........X.
....X....X.
........X..
..X....X...
..XXXXX....
...........");
            Assert.Equal(report, expect);
        }

        [Xunit.Fact]
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
            var stat = new StaticAnalysisMaps(p);

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
            stat = new StaticAnalysisMaps(p);
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
            stat = new StaticAnalysisMaps(p);
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
            stat = new StaticAnalysisMaps(p);
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
            stat = new StaticAnalysisMaps(p);
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
            stat = new StaticAnalysisMaps(p);
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
            stat = new StaticAnalysisMaps(p);
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
            stat = new StaticAnalysisMaps(p);
            Assert.True(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));
            
            
            // NOT_DEAD
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
            stat = new StaticAnalysisMaps(p);
            Assert.False(DeadMapAnalysis.DynamicCheck(stat, StateMaps.Create(p)));
        }

      

        [Xunit.Fact]
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

            var stat = new StaticAnalysisMaps(puz);
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