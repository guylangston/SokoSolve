﻿namespace SokoSolve.Core
{
    public static class SokoSolveApp
    {
        public static string Name = "SokoSolve - Sokoban Game and Solver (GPL)";
        public static string Version = "3.4.2";

        public static string VersionLog = @"
3.4.1 - Refactor NodeEvaluator to allow different NodeLookup strategies
3.4.0 - Fixed Fwd<->Rev match bug; better Task & Exit management
3.3.0 - Pool-vs-Tree dups/mismatch
3.2.0 - SolverNode memory 120b -> 88b + ISolverNodeFactory
3.1.2 - SolverNodeBinarySearchTree --pool bb:bst:lt";

    }
}
