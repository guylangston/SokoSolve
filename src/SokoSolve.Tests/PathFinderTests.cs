using System.Linq;
using NUnit.Framework;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Game;

namespace SokoSolve.Tests
{
    [TestFixture]
    public class PathFinderTests
    {
        [Test]
        public void FindPath()
        {
            var textPuzzle = Puzzle.Builder.FromLines(new[]
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


            var start = textPuzzle.Where(x => x.Underlying == 's').First().Position;
            var end = textPuzzle.Where(x => x.Underlying == 'e').First().Position;


            var result = PathFinder.Find(boundry, start, end);

            Assert.That(result, Is.Not.Null);

            Assert.That(result.ToString(), Is.EqualTo("LLUUUURUUL"));
        }

        [Test]
        public void Regression1()
        {
            var textPuzzle = Puzzle.Builder.FromLines(new[]
            {
                "~~~###",
                "~~## #",
                "~##s #",
                "##eX #",
                "#    #",
                "######"
            });

            var boundry = textPuzzle.ToMap('#', 'X');


            var start = textPuzzle.Where(x => x.Underlying == 's').First().Position;
            var end = textPuzzle.Where(x => x.Underlying == 'e').First().Position;


            var result = PathFinder.Find(boundry, start, end);

            Assert.That(result, Is.Not.Null);

            Assert.That(result.ToString(), Is.EqualTo("RDDLLU"));
        }
    }
}