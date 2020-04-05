# SokoSolve
Sokoban puzzle game and solver

![.NET Core](https://github.com/guylangston/SokoSolve/workflows/.NET%20Core/badge.svg)

The origonal project was started and hosted at 
- web: http://sokosolve.sourceforge.net/
- code: https://sourceforge.net/projects/sokosolve/

It was moved to github at a much later date and the C# code updated, however the GUI client has not been ported over.

## Getting Started

```
C:\Projects\> git clone https://github.com/guylangston/SokoSolve.git
C:\Projects\SokoSolve\src\SokoSolve.Tests\> dotnet build
C:\Projects\SokoSolve\src\SokoSolve.Tests\> dotnet test
```

## Game

I have not ported over the GUI or Console game client from the old source-forge repo. There are NO current plans to re-implement a user interface in WinForms. I have started a Web UI, but that is only for the solve.


## Solver

```pwsh
C:\Projects\SokoSolve\> benchmark.ps1
```

Almost all new work has been modernizing and updating the Solver. The solver is now at least 10x faster (with Time I will document the improvment and add some graphs).  As the solver is long-running and complex it is not a good fit for BenchmarkDotNet. Instead use the ``profile`` command:


![Benchmark run](./doc/Benchmark-2020-03-30-094045.png)

### Solver Progress / Benchmark Progress

To standardize measurement, I use the same puzzle, and a 3-min timeout.
```
          Ident: SQ1~P5
         Rating: 1068
~~~~~~~~~~~#####
~~~~~~~~~~##...#
~~~~~~~~~~#....#
~~~~####~~#.X.##
~~~~#..####X.X#~
~~~~#.....X.X.#~
~~~##.##.X.X.X#~
~~~#..O#..X.X.#~
~~~#..O#......#~
#####.#########~
#OOOO.P..#~~~~~~
#OOOO....#~~~~~~
##..######~~~~~~
~####~~~~~~~~~~~
```

Best Recent Result:
```
GUYZEN running RT:3.1.3 OS:'WIN 6.2.9200.0' Threads:32 RELEASE x64 'AMD Ryzen Threadripper 2950X 16-Core Processor '
Git: '2786d45 Added System.CommandLine' at 2020-04-05 10:23:54Z, v3.1.0
[SQ1~P5] NoSolution.  24,198,600 nodes at 134,415/s in 3 min.
```


## Library

The origonal code used a SQL database for most internal testing. This has been removed, as I don't want any external dependancies.
