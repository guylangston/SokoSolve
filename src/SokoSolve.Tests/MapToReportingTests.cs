using System;
using System.Text;
using SokoSolve.Core.Reporting;
using SokoSolve.Core.Solver;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests
{
    public class MapToReportingTests
    {
        private ITestOutputHelper outp;

        public MapToReportingTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void RenderStringTable()
        {
            var sb = new StringBuilder();
            ReportFactory.SolverStatistics()
                 .RenderTo(sb, new []
                 {
                     new SolverStatistics()
                     {
                         Name = "Item 1",
                         TotalNodes = 102345,
                         Started = new DateTime(2020, 1, 1),
                         Completed= (new DateTime(2020, 1, 1)).AddSeconds(12.3)
                     }, 
                     new SolverStatistics()
                     {
                         Name       = "Item 2",
                         TotalNodes = 35675,
                         Started    = new DateTime(2020, 1, 1),
                         Completed  = (new DateTime(2020, 1, 1)).AddSeconds(234.3)
                     },
                 });
            outp.WriteLine(sb.ToString());

            Assert.Equal(
@"Name   | TotalNodes | NodesPerSec | DurationInSec | 
=======|============|=============|===============|=
Item 1 |    102,345 |       -0.08 |         12.30 | 
Item 2 |     35,675 |       -0.00 |        234.30 | 
", sb.ToString());
            
        }
    }
}