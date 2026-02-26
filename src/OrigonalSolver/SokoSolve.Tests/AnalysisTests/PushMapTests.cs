using SokoSolve.Core;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;
using Xunit;

namespace SokoSolve.Tests.AnalysisTests
{
    public class PushMapTests
    {
        [Xunit.Fact]
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

            var staticMaps = new StaticMaps(
                Bitmap.Create(sample, '#'),
                Bitmap.Create(sample, ' ', 'a', 'b'),
                null,
                null
            );
            var stateMaps = new StateMaps(
                Bitmap.Create(sample,'A', 'B'),
                FloodFill.Fill(staticMaps.WallMap, Bitmap.FindPosition(sample, 'a')));

            var pushMap = PushMap.Find(staticMaps, stateMaps, Bitmap.FindPosition(sample, 'A'),
                Bitmap.FindPosition(sample, 'b'));

            report.WriteLine(pushMap);

            Assert.Equal(new TestReport(@".............
..X..........
.............
..X..........
.............
............."), report);
        }

        [Xunit.Fact]
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

            Assert.Equal(new TestReport(@"===================
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
"), report);
        }
    }
}
