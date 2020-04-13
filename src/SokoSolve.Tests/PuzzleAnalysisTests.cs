using System.Linq;
using NUnit.Framework.Internal;
using SokoSolve.Core;
using SokoSolve.Core.Analytics;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests
{
    public class PuzzleAnalysisTests
    {
        private readonly ITestOutputHelper outp;

        public PuzzleAnalysisTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void EnumerateFloor()
        {
            var p = Puzzle.Builder.DefaultTestPuzzle();
            var floor = p.ToMap(p.Definition.AllFloors);
            
            outp.WriteLine(floor.ToString());
            var enumFloor = StaticAnalysis.IndexPositions(floor);

            outp.WriteLine($"Floors : {enumFloor.Count} vs {floor.Size.X * floor.Size.Y}");
            var cc = 0;
            foreach (var f in enumFloor)
            {
                outp.WriteLine($"{cc++} -> {f}");
            }
        }
    }
}