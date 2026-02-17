using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;
using VectorInt;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests.AnalysisTests
{
    public class FindPathTests
    {
        private ITestOutputHelper outp;

        public FindPathTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }
        [Fact]
        public void PathExits()
        {
            var map = Bitmap.Create(new[]
            {
                "#####",
                "#   #",
                "# # #",
                "#####"
            }, x=>x == ' ');

            var p = SolverHelper.FindPath(map, new VectorInt2(3, 2), new VectorInt2(1, 2));
            Assert.NotNull(p);
            Assert.NotEmpty(p);
        }

        [Fact]
        public void Regression001()
        {
            var map = Bitmap.Create(
                @".........
.XXXXXXX.
.X.XXXXX.
......XX.
.....XX..
.....X...
.....X...
.........
.........", x => x == 'X');
            var start = new VectorInt2(6, 3);
            var end   = new VectorInt2(1, 2);
            var p     = SolverHelper.FindPath(map, start, end);
            Assert.NotNull(p);
            Assert.NotEmpty(p);

            var x = start;
            foreach (var step in p)
            {
                x += step;
                Assert.True(map[x]);
                //outp.WriteLine($"{step} => {x}");
            }
            Assert.Equal(end, x);

            end = new VectorInt2(5, 6);
            p   = SolverHelper.FindPath(map, start, end);
            Assert.NotNull(p);
            Assert.NotEmpty(p);

            x = start;
            foreach (var step in p)
            {
                x += step;
                Assert.True(map[x]);
                outp.WriteLine($"{step} => {x}");
            }
            Assert.Equal(end, x);

        }
    }
}
