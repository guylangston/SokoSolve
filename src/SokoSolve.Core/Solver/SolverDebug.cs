using Sokoban.Core.Debugger;

namespace Sokoban.Core.Solver
{
    public static class SolverDebug
    {
        public static readonly IDebugEvent DepthComplete = Setup(new NamedDebugEvent("DepthComplete"));
        public static readonly IDebugEvent Solution = Setup(new NamedDebugEvent("Solution"));

        private static IDebugEvent Setup(IDebugEvent item)
        {
            // Allow central control
            return item;
        }
    }
}