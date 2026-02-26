using System;
using System.Linq;
using Iced.Intel;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests.WIP
{
    public class ScratchTests
    {
        private ITestOutputHelper outp;

        public ScratchTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void ExpInCSharp()
        {
            var x = 1d;
            var e = Enumerable.Range(0, 100).Sum(n =>  Math.Pow(x, n) / Factorial((int)n));
            outp.WriteLine(e.ToString());

        }

        double Exp(double x) =>  Enumerable.Range(0, 100).Sum(n =>  Math.Pow(x, n) / Factorial((int)n));

        private double Factorial(int d)
        {
            double r = 1;
            for (var x = 2; x <= d; x++)
            {
                r *= (double)x;
            }

            return r;
        }
    }
}
