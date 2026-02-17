using System.IO;
using System.Numerics;

using SokoSolve.Core;
using SokoSolve.Drawing;
using Xunit;

namespace SokoSolve.Tests.Drawing
{
    public class PuzzleDrawingTests
    {
        [Fact]
        public void DrawDefault()
        {
            using (var outp = File.CreateText("puzzle.svg"))
            {
                var dia = new PuzzleDiagram();
                dia.Draw(outp, Puzzle.Builder.DefaultTestPuzzle(), new Vector2(50));
            }
        }

    }
}
