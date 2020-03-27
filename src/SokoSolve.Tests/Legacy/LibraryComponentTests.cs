using SokoSolve.Core.Lib;
using Xunit;

namespace SokoSolve.Tests.Legacy
{
    public class LibraryComponentTests
    {
        private readonly TestHelper helper = new TestHelper();

//        [Xunit.Fact]
//        [Ignore("Not sure")]
//        public void GenerateSolverRun()
//        {
//            var l = new LibraryComponent(helper.GetLibraryPath());
//            var lib1 = l.LoadLegacySokoSolve_SSX(l.GetPathData(@".\SokoSolve-v1\Sasquatch.ssx"));
//            var lib2 = l.LoadLegacySokoSolve_SSX(l.GetPathData(@".\SokoSolve-v1\SasquatchIII.ssx"));
//            var lib3 = l.LoadLegacySokoSolve_SSX(l.GetPathData(@".\SokoSolve-v1\SasquatchIV.ssx"));
//            var lib4 = l.LoadLegacySokoSolve_SSX(l.GetPathData(@".\SokoSolve-v1\Thinking Rabbit Inc, Origonal.ssx"));
//
//            var pool = new List<LibraryPuzzle>(lib1);
//            pool.AddRange(lib2);
//            pool.AddRange(lib3);
//            pool.AddRange(lib4);
//
//            ISokobanRepository rep = null; // TODO
//
//            var cc = 0;
//            foreach (var puzzle in pool.OrderBy(x => StaticAnalysis.CalculateRating(x.Puzzle)))
//            {
//                Console.WriteLine("Puzzle.{0}={1}", cc++, puzzle.Ident);
//                var dto = new PuzzleDTO
//                {
//                    Name = puzzle.Name,
//                    CharMap = puzzle.ToString(),
//                    Rating = (int) StaticAnalysis.CalculateRating(puzzle.Puzzle),
//                    Hash = puzzle.GetHashCode(),
//                    SourceIdent = puzzle.Ident.ToString(),
//                    Created = DateTime.Now,
//                    Modified = DateTime.Now
//                };
//                dto = rep.Confirm(dto);
//                Assert.NotEqual(0, dto.PuzzleId);
//            }
//
//            Assert.NotNull(lib1);
//            Assert.Equal(50, lib1.Count);
//        }

        [Xunit.Fact]
        public void LegaxySSX()
        {
            var l = new LibraryComponent(helper.GetLibraryPath());
            var lib = l.LoadLegacySokoSolve_SSX(l.GetPathData(@".\Sasquatch.ssx"));

            Assert.NotNull(lib);
            Assert.Equal(50, lib.Count);
        }
    }
}