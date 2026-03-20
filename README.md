# SokoSolve

A high-performance Sokoban puzzle solver designed for very large search trees. For background, see my [blog:SokoSolve](https://www.guylangston.net/Blog/Article/SokoSolve).

## About

SokoSolve is a Sokoban solver built from the ground up to handle complex puzzles with massive search spaces. The current implementation focuses on:

- **Low GC pressure** through custom structs and pooling
- **Multi-threading support** for faster solving
- **Clean architecture** with lessons learned from previous implementations
- **Zero dependencies** for maximum portability

## Quick Start

### Prerequisites

- .NET SDK (6.0 or later)
- Git

### Clone and Build

```bash
git clone https://github.com/guylangston/SokoSolve.git
cd SokoSolve/src/LargeSearchSolver
make build-release
```

## Running the Solver

### Basic Usage

Navigate to the console application directory and run:

```bash
cd src/LargeSearchSolver/SokoSolve.LargeSearchSolver.Console
dotnet run -c Release -- solve --puzzle SQ1~P7
```

### Command Options

```bash
# Show all available commands
dotnet run -c Release -- --help

# Show solve command options
dotnet run -c Release -- solve --help
```

**Available Commands:**

```
Usage:
  sokosolve [command] [options]

Options:
  --pid           Write PID to file ./sokosolve.pid
  -?, -h, --help  Show help and usage information
  --version       Show version information

Commands:
  solve  Solve Puzzle
```

**Solve Command Options:**

```
Options:
  -p, --puzzle <puzzle> (REQUIRED)  Example SQ1~P5
  -m, --maxNodes <maxNodes>         Max Nodes then abort
  -d, --maxDepth <maxDepth>         Max Depth then abort
  -R, --minRating <minRating>       Min Puzzle Rating to attempt
  -r, --maxRating <maxRating>       Max Puzzle Rating to attempt
  -t, --maxTime <maxTime>           Max Time (seconds) then abort
  -E, --experimental                Use experimental features
  -I, --non-interactive             Non-Interactive console session (no cursor/colours)
  --veryLarge                       Very large search attempt
  --tags <tags>                     General-purpose tags, comma-separated
  --stop-on-swap                    Stop on memory swap
  --exhaustive                      Exhaustive search mode
  -?, -h, --help                    Show help and usage information
```

**Examples:**

```bash
# Solve a specific puzzle with rating filters and time limit
dotnet run -c Release -- solve --puzzle SQ1 --minRating 40 --maxTime 600

# Run the standard benchmark puzzle
dotnet run -c Release -- solve --puzzle SQ1~P7

# Solve with depth and node limits
dotnet run -c Release -- solve --puzzle SQ1~P5 --maxDepth 100 --maxNodes 1000000
```

**Typical Output:**
```txt
===[Solver Header]===    solve --puzzle SQ1~P7 --stop-on-swap --veryLarge
PID: 91954
guyzen-arch 'AMD Ryzen Threadripper 2950X 16-Core Processor' OS:Unix6.19.8.1 dotnet:10.0.3 Threads:32 x64 DEBUG
GIT-LOG1: commit 09068861383bc88a284be0bb38814ca20379693c
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Fri Mar 20 17:14:21 2026 +0000
GIT-LOG1:     docs: updated legacy README
MemTotal:       65704840 kB
MemFree:        45957436 kB
MemAvailable:   55365360 kB
SwapCached:            0 kB
SwapTotal:       4194300 kB
SwapFree:        4194300 kB
sizeof(NodeStruct)=84. TheorticalNodeLimit=674,924,641. sizeof(NodeStructWord)=2
Available Puzzles: 1
===[Body]===    Bob's Cottage
Puzzle:              Bob's Cottage
Ident:               SQ1~P7
Rating:              60
Size:                (13,10)
Contraints:          StopOnSolution,StopOnSwap
Known-Size-Solution: 15,529,013
~~###########
~##.....#..P#
###.X.XX#...#
#.##X....XX.#
#..#..X.#...#
######.######
#OO.OOX.#$##~
#.OO....###~~
#..OO#####~~~
#########~~~~
CMP NodeStruct:                                                        v1.1:Nested-MyBitmapStruct,CustomFloodFill
CMP SolverCoordinator->SolverCoordinator:                              LS-v1.2--Forward+Reverse+SingleThread WithPeek
CMP INodeHeap->NodeHeap:                                               BlockSize: 10000000
CMP INodeBacklog->NodeBacklog:                                         BlockSize: 1000000
CMP ILNodeLookup->LNodeLookupCompound:                                 Sharing[16] -> Dynamic=LNodeLookupBlackRedTree(312500), Immutable[LNodeLookupImmutable]
CMP ILNodeStructEvaluator->LNodeStructEvaluatorForwardDeadChecks(fwd): v1.4:Dead
CMP ILNodeStructEvaluator->LNodeStructEvaluatorReverse(rev):           v0.1
CMP INodeHashCalculator:                                               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
CMP ISolverCoordinatorFactory->SolverCoordinatorFactory:               ,MEMORY,VERYLARGE

Completed:   00:02:19.5385020
Memory used: 1275MB
Total nodes: 4,737,089 at 33,948.3nodes/sec
Dead:        714,608
Result:      SOLUTION!
===[FOOTER]===
Puzzle | Rating | Time(sec) | Nodes   | Solutions | Machine     | Version                               |
SQ1~P7 | 60     | 139.5     | 4737089 | 1         | guyzen-arch | LS-v1.2--Forward+Reverse+SingleThread |
```

### Using Makefile

From the `src/LargeSearchSolver` directory:

```bash
# Run standard benchmark (SQ1~P7 puzzle)
make bench

# Run all tests
make test

# Run micro benchmarks
make micro

# Clean, restore, test, and benchmark
make
```

## Architecture

The solver implements a sophisticated search strategy with:

- **NodeHeap**: First-class heap with pooling for memory efficiency
- **Custom structs**: Reduced garbage collection overhead
- **SolverCoordinator**: Multi-threaded solver control with cancellation support
- **Dual search trees**: Forward and reverse solving strategies
- **Thread-safe operations**: Lock-free where possible for maximum performance

### Core Components

- `NodeStruct` - Fundamental search unit
- `NodeHeap` - Memory-pooled node allocation for both forward and reverse trees
- `SolverCoordinator` - Main loop control (start/stop/pause)
- `SolverStrategy` - Forward/reverse/crate movement strategies
- `PrimaryBacklog` - Queue of unevaluated nodes

## Development

### Project Structure

```
src/LargeSearchSolver/
├── SokoSolve.LargeSearchSolver/          # Core solver library
├── SokoSolve.LargeSearchSolver.Console/  # Command-line interface
├── SokoSolve.LargeSearchSolver.Tests/    # Unit tests
├── SokoSolve.LargeSearchSolver.MicroBenchmarks/  # Performance tests
└── SokoSolve.Primitives/                 # Base types and utilities
```

### Building and Testing

```bash
# Build in debug mode
make build

# Build in release mode
make build-release

# Run tests
make test

# Clean build artifacts
make clean
```

## History

The original project was started and hosted at SourceForge ([code](https://sourceforge.net/projects/sokosolve/), [web](https://web.archive.org/web/20180315141727/http://sokosolve.sourceforge.net/)).

After many years, it was moved to GitHub, upgraded, and refactored. The GUI and database dependencies were removed in favor of a lightweight, high-performance core solver.

The current implementation (`src/LargeSearchSolver`) is a ground-up rewrite focused on solving very large search trees efficiently, replacing the original solver in `src/OrigonalSolver`.
