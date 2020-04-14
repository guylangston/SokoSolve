using System.Linq;
using SokoSolve.Core.Primitives;
using VectorInt;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests
{
    public class BitmapTests
    {
        private ITestOutputHelper outp;

        public BitmapTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        public void CheckBasic(IBitmap b)
        {
            b[1, 1] = true;
            b[2, 2] = false;
            
            Assert.True(b[1,1]);
            Assert.False(b[2,2]);

            var p = b.Where(x=>x.Item2).ToArray();
            Assert.Single(p);
            Assert.Equal(new VectorInt2(1,1), p[0].Item1);
        }

        [Fact]
        public void Bitmap()
        {
            CheckBasic(new Bitmap(10, 10));
        }
        
        [Fact]
        public void BitmapByteSeq()
        {
            CheckBasic(new BitmapByteSeq(new VectorInt2(10, 10)));
        }
        
        [Fact]
        public void BitmapByteSeq_Shared()
        {
            var shared = new byte[100 + 100];
            CheckBasic(new BitmapByteSeq(shared, 0, new VectorInt2(10, 10)));
            CheckBasic(new BitmapByteSeq(shared, 100, new VectorInt2(10, 10)));
        }
    }
}