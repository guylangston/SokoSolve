using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework.Internal.Execution;
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

        void CheckBasic(IBitmap b)
        {
            b[1, 1] = true;
            b[2, 2] = false;

            Assert.True(b[1,1]);
            Assert.False(b[2,2]);
            Assert.True(b[new VectorInt2(1,1)]);
            Assert.False(b[new VectorInt2(2,2)]);

            Assert.Equal(1, b.Count());

            var asList = b.ForEach().ToList();
            Assert.Contains((new VectorInt2(1, 1), true), asList);
            Assert.Contains((new VectorInt2(2, 2), false), asList);

            var hash = b.GetHashCode();
            Assert.NotEqual(0, hash);

            var clone = new Bitmap(b);
            Assert.Equal(b, clone);
            Assert.True(b.Equals(clone));
            Assert.Equal(0, b.CompareTo(clone));

            Assert.Equal(b, b.BitwiseOR(b));
            Assert.Equal(b, b.BitwiseAND(b));

            Assert.True(b.SizeInBytes() > 0);
        }

        void CheckUsingFactory<T>(Func<VectorInt2, T> create) where T : IBitmap
        {
            var b1 = DrawBox(create(new VectorInt2(10, 10)));
            var b2 = DrawBox(create(new VectorInt2(10, 10)));

            Assert.Equal(b1, b2);
            Assert.Equal(b1.GetHashCode(), b2.GetHashCode());
            Assert.Equal(b1.ForEach().ToList(), b2.ForEach().ToList());

            var b3 = DrawBox(create(new VectorInt2(11, 11)));
            Assert.NotEqual(b1, b3);
            Assert.NotEqual(b1.GetHashCode(), b3.GetHashCode());
            Assert.NotEqual(b1.ForEach().ToList(), b3.ForEach().ToList());

            var b4 = DrawBox(create(new VectorInt2(10, 10)));
            b4[5, 5] = true;
            Assert.Equal(b1.Count + 1, b4.Count);
            Assert.False(b1.Equals(b4));
            Assert.NotEqual(b1, b4);
            Assert.NotEqual(b1.GetHashCode(), b4.GetHashCode());
            Assert.NotEqual(b1.ForEach().ToList(), b4.ForEach().ToList());

            outp.WriteLine(b4.ToString());

        }

        public static IBitmap DrawBox(IBitmap b)
        {
            for (int i = 0; i < b.Width; i++)
            {
                b[i, 0]            = true;
                b[i, b.Height - 1] = true;
            }

            for (int i = 0; i < b.Height; i++)
            {
                b[0, i]           = true;
                b[b.Width - 1, i] = true;
            }

            return b;
        }

        [Fact]
        public void Bitmap()
        {
            CheckBasic(new Bitmap(10, 10));
            CheckUsingFactory(x=>new Bitmap(x));
        }

        [Fact]
        public void BitmapByteSeq()
        {
            CheckBasic(new BitmapByteSeq(new VectorInt2(10, 10)));
            CheckUsingFactory(x=>new BitmapByteSeq(x));
        }

        [Fact]
        public void BitmapMaskedIndex()
        {
            var stringMask10_10 =
                @"##########
##########
##########
##########
##########
##########
##########
##########
##########
##########";
            var ref10 = SokoSolve.Core.Primitives.Bitmap.Create(stringMask10_10, x=>x != '.');

            Assert.Equal(new VectorInt2(10), ref10.Size );
            var master = new BitmapMaskedIndex.Master(ref10);

            var stringMask11_11 =
                @"###########
###########
###########
###########
###########
###########
###########
###########
###########
###########
###########";
            var ref11 = SokoSolve.Core.Primitives.Bitmap.Create(stringMask11_11, x=>x != '.');
            Assert.Equal(new VectorInt2(11), ref11.Size );
            var master11 = new BitmapMaskedIndex.Master(ref11);

            CheckBasic(new BitmapMaskedIndex(master));
            CheckUsingFactory(x =>
            {
                if (x.X == 10 && x.Y == 10) return (IBitmap)new BitmapMaskedIndex(master);
                if (x.X == 11 && x.Y == 11) return (IBitmap)new BitmapMaskedIndex(master11);
                throw new NotSupportedException(x.ToString());

            });

        }
    }
}
