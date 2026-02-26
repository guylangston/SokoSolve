namespace SokoSolve.Core
{
    public static class SokoSolveApp
    {
        public const string Name = "SokoSolve - Sokoban Game and Solver (GPL)";
        public const string Version = "3.4.3";

        public const string VersionLog = @"
3.4.3 - net10
3.4.1 - Refactor NodeEvaluator to allow different NodeLookup strategies
3.4.0 - Fixed Fwd<->Rev match bug; better Task & Exit management
3.3.0 - Pool-vs-Tree dups/mismatch
3.2.0 - SolverNode memory 120b -> 88b + ISolverNodeFactory
3.1.2 - SolverNodeBinarySearchTree --pool bb:bst:lt";

    }
}
