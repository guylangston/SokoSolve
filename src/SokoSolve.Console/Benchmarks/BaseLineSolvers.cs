using System;
using BenchmarkDotNet.Attributes;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;
using SokoSolve.Core.Solver.NodeFactory;
using SokoSolve.Core.Solver.Solvers;

namespace SokoSolve.Console.Benchmarks
{
    public class BaseLineSolvers
    {
        
        private static SolverCommand CreateCommand() => new SolverCommand(
            Puzzle.Builder.DefaultTestPuzzle(),
            PuzzleIdent.Temp(), 
            ExitConditions.OneMinute(),
            SolverContainerByType.DefaultEmpty
        );

        [Benchmark]
        public void ForwardSingle()
        {
            var solverCommand = CreateCommand();
            var solver        = new SingleThreadedForwardSolver(new SolverNodePoolingFactoryDefault());
            var solverState   = solver.Init(solverCommand);
            solver.Solve(solverState);
            if (!solverState.HasSolution) throw new Exception();
        }
       

        [Benchmark]
        public void ReverseSingle()
        {
            var solverCommand = CreateCommand();
            var solver        = new SingleThreadedReverseSolver(new SolverNodePoolingFactoryDefault());
            var solverState   = solver.Init(solverCommand);
            solver.Solve(solverState);
            if (!solverState.HasSolution) throw new Exception();
        }
        
        [Benchmark]
        public void ForwardReverseSingle()
        {
            var solverCommand = CreateCommand();
            var solver        = new SingleThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault());
            var solverState   = solver.Init(solverCommand);
            solver.Solve(solverState);
            if (!solverState.HasSolution) throw new Exception();
        }
    }


}
/*

// LAST RUN

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
AMD Ryzen Threadripper 2950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=3.1.201
  [Host]     : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT

|               Method |      Mean |    Error |   StdDev |
|--------------------- |----------:|---------:|---------:|
|        ForwardSingle | 107.11 ms | 1.258 ms | 1.176 ms |
|        ReverseSingle |  35.48 ms | 0.169 ms | 0.141 ms |
| ForwardReverseSingle |  32.16 ms | 0.079 ms | 0.074 ms |

----------------------------

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