using System.Linq;
using NUnit.Framework;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Puzzle;

namespace SokoSolve.Tests
{
    [TestFixture]
    public class PathFinderTests
    {
        [Test]
        public void FindPath()
        {
            var textPuzzle = new Puzzle(new[]
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
            });

            var boundry = textPuzzle.ToMap('#', 'X');


            var start = textPuzzle.Where(x => x == 's').First().Position;
            var end = textPuzzle.Where(x => x == 'e').First().Position;


            var result = PathFinder.Find(boundry, start, end);

            Assert.That(result, Is.Not.Null);

            Assert.That(result.ToString(), Is.EqualTo("LLUUUURUUL"));
        }

        [Test]
        public void Regression1()
        {
            var textPuzzle = new Puzzle(new[]
            {
                "~~~###",
                "~~## #",
                "~##s #",
                "##eX #",
                "#    #",
                "######"
            });

            var boundry = textPuzzle.ToMap('#', 'X');


            var start = textPuzzle.Where(x => x == 's').First().Position;
            var end = textPuzzle.Where(x => x == 'e').First().Position;


            var result = PathFinder.Find(boundry, start, end);

            Assert.That(result, Is.Not.Null);

            Assert.That(result.ToString(), Is.EqualTo("RDDLLU"));
        }
    }
}