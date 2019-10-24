using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using Xunit;

namespace SokoSolve.Tests.NUnitTests
{
    public class PathFinderTests
    {
        [Xunit.Fact]
        public void FindPath()
        {
            var textPuzzle = new[]
            {
                "~~~###~~~~~",
                "~~## #~####",
                "~##e ###  #",
                "## X      #",
                "#    X #  #",
                "### X###  #",
                "~~#  #    #",
                "~## ## # ##",
                "~#   s  ##~",
                "~#     ##~~",
                "~#######~~~"
            };

            var boundry = Bitmap.Create(textPuzzle, x=>x == '#'  || x == 'X');
            var start = Bitmap.FindPosition(textPuzzle, 's');
            var end = Bitmap.FindPosition(textPuzzle, 'e');


            var result = PathFinder.Find(boundry, start, end);

            Assert.NotNull(result);

            Assert.Equal("LLUUUURUUL", result.ToString());
        }

        [Xunit.Fact]
        public void Regression1()
        {
            var textPuzzle = new[]
            {
                "~~~###",
                "~~## #",
                "~##s #",
                "##eX #",
                "#    #",
                "######"
            };

            var boundry = Bitmap.Create(textPuzzle, x=>x == '#' || x == 'X');
            var start = Bitmap.FindPosition(textPuzzle, 's');
            var end = Bitmap.FindPosition(textPuzzle, 'e');
            
            var result = PathFinder.Find(boundry, start, end);

            Assert.NotNull(result);

            Assert.Equal("RDDLLU", result.ToString());
        }
    }
}