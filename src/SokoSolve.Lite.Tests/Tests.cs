using System;
using System.ComponentModel.DataAnnotations;
using SokoSolve.Lite;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Lite.Tests
{
    public class MapBuilderTests
    {
        private ITestOutputHelper outp;

        public MapBuilderTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void Parse()
        {
            var d1 = MapBuilder.Default;

            var d = d1.ToString();

            outp.WriteLine(d);

            Assert.Equal(
                @"#~~###~~~~#
~~##.#~####
~##..###..#
##.X......#
#...PX.#..#
###.X###..#
~~#..#OO..#
~##.##O#.##
~#......##~
~#.....##~~
########~~~", d);
        }
    }
}