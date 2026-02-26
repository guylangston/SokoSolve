using SokoSolve.Console.Benchmarks;
using SokoSolve.Core.Solver.Lookup;
using SokoSolve.Core.Util;

namespace SokoSolve.Console
{
    public static class ScratchCommand      // Work In Progress
    {
        public static void Run(string target)
        {
            //CalcCollisions_1mil();
            Play_FloodSplill();

        }
        private static void Play_FloodSplill()
        {
            var x = new FloodFillMicro();
            x.NativeC();
        }

        private static void CalcCollisions_1mil()
        {

            System.Console.WriteLine($"Creating Nodes...");

            var nodes = SolverNodeBuilder.BuildSolverNodes(1_000_000);

            System.Console.WriteLine($"Creating OBST...");
            var obst = new NodeLookupOptimisticLockingBinarySearchTree();
            foreach (var node in nodes)
            {
                obst.Add(node);
            }

            System.Console.WriteLine($"Count: {obst.Inner.Count}, Coll: {obst.Inner.CountHashCollision} => {obst.Inner.CountHashCollision * 100f / obst.Inner.Count}");
        }
    }
}
