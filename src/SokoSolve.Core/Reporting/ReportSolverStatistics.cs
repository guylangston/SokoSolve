using SokoSolve.Core.Solver;
using TextRenderZ.Reporting;

namespace SokoSolve.Core.Reporting
{
    public static class ReportFactory
    {
        public static IMapToReporting<SolverStatistics> SolverStatistics()
            => new MapToReporting<SolverStatistics>()
               .AddColumn("Name")
               .AddColumn("TotalNodes")
               .AddColumn("NodesPerSec")
               .AddColumn("DurationInSec");

    }
}