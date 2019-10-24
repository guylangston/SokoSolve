using NUnit.Framework;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Tests.NUnitTests
{
    [TestFixture]
    public class FloodFillTests
    {
        [Test]
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
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
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
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
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
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}