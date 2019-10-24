using NUnit.Framework;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Tests.NUnitTests
{
    [TestFixture]
    public class PathFinderTests
    {
        [Test]
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

            Assert.That(result, Is.Not.Null);

            Assert.That(result.ToString(), Is.EqualTo("LLUUUURUUL"));
        }

        [Test]
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

            Assert.That(result, Is.Not.Null);

            Assert.That(result.ToString(), Is.EqualTo("RDDLLU"));
        }
    }
}