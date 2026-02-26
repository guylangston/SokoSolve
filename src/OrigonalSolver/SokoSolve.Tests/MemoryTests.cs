using System.Runtime.InteropServices;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;
using VectorInt;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests
{
    public class MemoryTests
    {
        private ITestOutputHelper outp;

        public MemoryTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        // [Fact]
        // public void SizeOf_SolverNode()
        // {
        //     var s = new SolverNode(new VectorInt2(), new VectorInt2(), new VectorInt2(), new VectorInt2(),
        //         new Bitmap(15, 15), new Bitmap(15, 15), 1, null);
        //     outp.WriteLine($"Size of: {nameof(SolverNode)} = {Marshal.SizeOf(s)}");
        // }
    }
}