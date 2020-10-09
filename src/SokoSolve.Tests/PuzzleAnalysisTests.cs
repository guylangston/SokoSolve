using System.Linq;
using NUnit.Framework.Internal;
using SokoSolve.Core;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
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
        public void WIP_ExploringImprovedBitmap()
        {
            var p = Puzzle.Builder.DefaultTestPuzzle();
            var floor = p.ToMap(p.Definition.AllFloors);
            
            outp.WriteLine($"{floor.GetType().Name} = {floor.SizeInBytes()} bytes");
            var alt = new BitmapByteSeq(floor);
            
            outp.WriteLine($"{alt.GetType().Name}  = {alt.SizeInBytes()}");
            
            
            var enumFloor = StaticAnalysis.IndexPositions(floor);
            outp.WriteLine($"Floors : {enumFloor.Count}={enumFloor.Count /8 + 1}bytes vs {floor.Size.X * floor.Size.Y}");
           
        }
    }
}