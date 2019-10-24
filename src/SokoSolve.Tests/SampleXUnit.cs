using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests
{
    public class SampleXUnit
    {
        private ITestOutputHelper outp;

        public SampleXUnit(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void CheckRunner()
        {
            outp.WriteLine("Hello World");
        }
    }
}