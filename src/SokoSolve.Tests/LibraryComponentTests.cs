using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sokoban.Core.Analytics;
using Sokoban.Core.Library;
using Sokoban.Core.Library.DB;
using Sokoban.Core.PuzzleLogic;
using Sokoban.Core.Solver;

namespace SokoSolve.Tests
{
    [TestFixture]
    public class LibraryComponentTests
    {
        [Test]
        public void LegaxySSX()
        {
            var l = new LibraryComponent();
            var lib=l.LoadLegacySokoSolve_SSX(l.GetPathData(@".\LegacySSX\Sasquatch.ssx"));

            Assert.That(lib, Is.Not.Null);
            Assert.That(lib.Count, Is.EqualTo(50));
        }

        [Test]
        [Ignore]
        public void GenerateSolverRun()
        {
            var l = new LibraryComponent();
            var lib1 = l.LoadLegacySokoSolve_SSX(l.GetPathData(@".\LegacySSX\Sasquatch.ssx"));
            var lib2 = l.LoadLegacySokoSolve_SSX(l.GetPathData(@".\LegacySSX\SasquatchIII.ssx"));
            var lib3 = l.LoadLegacySokoSolve_SSX(l.GetPathData(@".\LegacySSX\SasquatchIV.ssx"));
            var lib4 = l.LoadLegacySokoSolve_SSX(l.GetPathData(@".\LegacySSX\Thinking Rabbit Inc, Origonal.ssx"));

            var pool = new List<LibraryPuzzle>(lib1);
            pool.AddRange(lib2);
            pool.AddRange(lib3);
            pool.AddRange(lib4);

            var rep = new SokoDBRepository();

            int cc = 0;
            foreach (var puzzle in pool.OrderBy(x=>StaticAnalysis.CalculateRating(x)))
            {
                Console.WriteLine("Puzzle.{0}={1}", cc++, puzzle.Ident);
                var dto = new PuzzleDTO()
                {
                    Name = puzzle.Name,
                    CharMap = puzzle.ToString(),
                    Rating = (int)StaticAnalysis.CalculateRating(puzzle),
                    Hash = puzzle.GetHashCode(),
                    SourceIdent = puzzle.Ident.ToString(),
                    Created = DateTime.Now,
                    Modified = DateTime.Now
                };
                dto = rep.Confirm(dto);
                Assert.That(dto.PuzzleId, Is.Not.EqualTo(0));
            }

            Assert.That(lib1, Is.Not.Null);
            Assert.That(lib1.Count, Is.EqualTo(50));


        }

        [Test]
        public void LoadThenSave()
        {
            var l = new LibraryComponent();
            var p = l.LoadProfile(l.GetPathData("Profiles\\guy.profile"));
            Assert.That(p, Is.Not.Null);

            Console.WriteLine(l.SaveProfile(p, l.GetPathData("Profiles\\guy.profile")));
        }
    }
}
