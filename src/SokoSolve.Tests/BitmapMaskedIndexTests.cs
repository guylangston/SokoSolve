using System;
using System.ComponentModel;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using TextRenderZ;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests
{
    public class BitmapMaskedIndexTests
    {
        private ITestOutputHelper outp;

        public BitmapMaskedIndexTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void CanConstructMask_Default()
        {
            var p = Puzzle.Builder.DefaultTestPuzzle();
            BuildMaster(p);
        }
        
        [Fact]
        public void CanConstructMask_Ref_Solved()
        {
            var p = Puzzle.Builder.Reference_Solved_SQ1P15();
            BuildMaster(p);
        }
        
        [Fact]
        public void CanConstructMask_Ref_UnSolved()
        {
            var p = Puzzle.Builder.Reference_UnSolved_SQ1P13();
            BuildMaster(p);
        }

        private BitmapMaskedIndex.Master BuildMaster(Puzzle p)
        {
            var floors = p.ToMap(p.Definition.AllFloors);
            var master = new BitmapMaskedIndex.Master(floors);

            outp.WriteLine(master.Reference.ToString());

            outp.WriteLine($"Size: {master}");
            outp.WriteLine($"Size.Bitmap: {Bitmap.SizeInBytes(master.Reference.Size)}");
            outp.WriteLine($"Size.BitmapByteSeq: {BitmapByteSeq.SizeInBytes(master.Reference.Size)}");

            outp.WriteLine(new FluentString().ForEach(master.MapIn));
            outp.WriteLine(new FluentString(Environment.NewLine).ForEach(master.MapOut.WithIndex()));

            return master;
        }
        
        
    }
}