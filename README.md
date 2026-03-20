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
