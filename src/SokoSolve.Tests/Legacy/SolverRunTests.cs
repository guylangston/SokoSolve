using System;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;

namespace SokoSolve.Tests.Legacy
{
    public class SolverRunTests
    {
        private readonly TestHelper helper = new TestHelper();

        [Xunit.Fact]
        public void CanLoadAll()
        {
            Console.WriteLine(Environment
                .CurrentDirectory); // C:\Projects\SokoSolve\src\SokoSolve.Tests\bin\Debug\netcoreapp3.0

            var res = new SolverRun();
            res.Load(new LibraryComponent(helper.GetDataPath()), "SolverRun-Default.tff");
        }
    }
}