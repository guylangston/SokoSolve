using System;
using NUnit.Framework;
using SokoSolve.Core.Library;
using SokoSolve.Core.Solver;

namespace SokoSolve.Tests.NUnitTests
{
    [TestFixture]
    public class SolverRunTests
    {
        private readonly TestHelper helper = new TestHelper();

        [Test]
        public void CanLoadAll()
        {
            Console.WriteLine(Environment
                .CurrentDirectory); // C:\Projects\SokoSolve\src\SokoSolve.Tests\bin\Debug\netcoreapp3.0

            var res = new SolverRun();
            res.Load(new LibraryComponent(helper.GetDataPath()), "SolverRun-Default.tff");
        }
    }
}