using NUnit.Framework;
using SokoSolve.Core.Solver;

namespace SokoSolve.Tests
{
    [TestFixture]
    public class SolverRunTests
    {
        [Test]
        public void CanLoadAll()
        {
            var res = new SolverRun();
            res.Load(null, "SolverRun-Default.tff");
        }


      
    }
}