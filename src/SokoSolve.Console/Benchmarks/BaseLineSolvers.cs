using System;
using BenchmarkDotNet.Attributes;
using SokoSolve.Core.Game;
using SokoSolve.Core.Solver;

namespace SokoSolve.Console.Benchmarks
{
    public class BaseLineSolvers
    {
        
        [Benchmark]
        public void ForwardSingle()
        {
            var solverCommand = new SolverCommand()
            {
                Puzzle         = Puzzle.Builder.DefaultTestPuzzle(),
                ExitConditions = ExitConditions.Default3Min,
            };
            var solver      = new SingleThreadedForwardSolver();
            var solverState = solver.Init(solverCommand);
            solver.Solve(solverState);
            if (!solverState.HasSolution) throw new Exception();
        }
        
        [Benchmark]
        public void ReverseSingle()
        {
            var solverCommand = new SolverCommand()
            {
                Puzzle         = Puzzle.Builder.DefaultTestPuzzle(),
                ExitConditions = ExitConditions.Default3Min,
            };
            var solver      = new SingleThreadedReverseSolver();
            var solverState = solver.Init(solverCommand);
            solver.Solve(solverState);
            if (!solverState.HasSolution) throw new Exception();
        }
        
        [Benchmark]
        public void ForwardReverseSingle()
        {
            var solverCommand = new SolverCommand()
            {
                Puzzle         = Puzzle.Builder.DefaultTestPuzzle(),
                ExitConditions = ExitConditions.Default3Min,
            };
            var solver      = new SingleThreadedForwardReverseSolver();
            var solverState = solver.Init(solverCommand);
            solver.Solve(solverState);
            if (!solverState.HasSolution) throw new Exception();
        }
    }
}
/*

// LAST RUN

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
AMD Ryzen Threadripper 2950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=3.0.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT


|               Method |      Mean |     Error |    StdDev |
|--------------------- |----------:|----------:|----------:|
|        ForwardSingle | 117.97 ms | 0.2189 ms | 0.2048 ms |
|        ReverseSingle |  39.28 ms | 0.1142 ms | 0.1068 ms |
| ForwardReverseSingle |  29.06 ms | 0.2679 ms | 0.2506 ms |

*/