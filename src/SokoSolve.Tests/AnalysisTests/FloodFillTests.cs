using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;
using Xunit;

namespace SokoSolve.Tests.AnalysisTests
{

    public class FloodFillTests
    {
        [Xunit.Fact]
        public void Open()
        {
            var bountry = Bitmap.Create(new[]
            {
                "~~~# #~~~~~",
                "~~## #~####",
                "~##  ###  #",
                "## X      #",
                "#    X #  #",
                "### X###  #",
                "~~#  #    #",
                "~## ## # ##",
                "~#      ##~",
                "~#     ##~~",
                "~#######~~~"
            });

            var expected = Bitmap.Create(new[]
            {
                "~~~# #~~~~~",
                "~~## #~####",
                "~##  ###  #",
                "## X      #",
                "#    X #  #",
                "### X###  #",
                "~~#  #    #",
                "~## ## # ##",
                "~#      ##~",
                "~#     ##~~",
                "~#######~~~"
            }, x => x == ' ');

            var start = new VectorInt2(4, 4);

            var result = FloodFill.Fill(bountry, start);
            Assert.Equal(expected, result);
        }

        [Xunit.Fact]
        public void Sample()
        {
            var bountry = Bitmap.Create(new[]
            {
                "~~~###~~~~~",
                "~~## #~####",
                "~##  ###  #",
                "## X      #",
                "#    X #  #",
                "### X###  #",
                "~~#  #    #",
                "~## ## # ##",
                "~#      ##~",
                "~#     ##~~",
                "~#######~~~"
            });

            var expected = Bitmap.Create(new[]
            {
                "~~~###~~~~~",
                "~~## #~####",
                "~##  ###  #",
                "## X      #",
                "#    X #  #",
                "### X###  #",
                "~~#  #    #",
                "~## ## # ##",
                "~#      ##~",
                "~#     ##~~",
                "~#######~~~"
            }, x => x == ' ');

            var start = new VectorInt2(4, 4);

            var result = FloodFill.Fill(bountry, start);
            Assert.Equal(expected, result);
        }

        [Xunit.Fact]
        public void TwoRooms()
        {
            var bountry = Bitmap.Create(new[]
            {
                "#########",
                "#  ######",
                "#  ###  #",
                "######  #",
                "#########"
            }, x => x == ' ');

            var expected = Bitmap.Create(new[]
            {
                "#########",
                "#oo######",
                "#oo###  #",
                "######  #",
                "#########"
            }, x => x == 'O');

            var start = new VectorInt2(1, 1);

            var result = FloodFill.Fill(bountry, start);
            Assert.Equal(expected, result);
        }
    }
}
