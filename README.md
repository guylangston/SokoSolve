# SokoSolve
Sokoban puzzle game and solver

![.NET Core](https://github.com/guylangston/SokoSolve/workflows/.NET%20Core/badge.svg)

The original project was started and hosted at SourceForge ([code](https://sourceforge.net/projects/sokosolve/), [web](https://web.archive.org/web/20180315141727/http://sokosolve.sourceforge.net/)) 

After many years, it was moved to github, upgraded and stripped down (dropped the WinForms GUI).

## Getting Started

```cmd
C:\Projects\> git clone https://github.com/guylangston/SokoSolve.git
C:\Projects\SokoSolve\> benchmark.ps1
```

## Game

I have not ported over the GUI or Console game client from the old source-forge repo. There are NO current plans to re-implement a user interface in WinForms. I have started a Web UI, but that is only for the solve.


## Solver

```pwsh
C:\Projects\SokoSolve\> benchmark.ps1
```

Almost all new work has been modernizing and updating the Solver. The solver is now at least 10x faster (with Time I will document the improvement and add some graphs).  As the solver is long-running and complex it is not a good fit for BenchmarkDotNet. Instead use the ``profile`` command:

![Benchmark run](./doc/Benchmark-2020-03-30-094045.png)


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

### Solver Progress / Benchmark Progress

```
   Args: --min 5 --sec 0 --solver fr! --pool bb:lock:sl:lt,bb:lock:bucket --min-rating 200 --max-ratring 800
 Report: C:\Projects\SokoSolve\src\SokoSolve.Console\results\benchmark--2020-04-08T19-26-46.txt
Machine: GUYZEN running RT:3.1.3 OS:'WIN 6.2.9200.0' Threads:32 RELEASE x64 'AMD Ryzen Threadripper 2950X 16-Core Processor '
Version: '[DIRTY] c724b04 Progress notifications, rev:191' at 2020-04-08 20:38:35Z, v3.1.1

Solver | Pool           | Puzzle  | Result   | Solutions | Statistics                                        | 
=======|================|=========|==========|===========|===================================================|=
fr!    | bb:lock:sl:lt  | SQ1~P27 | Solution | 3         |       16,012 nodes at   12,825/s in 1.2 sec       | 
fr!    | bb:lock:sl:lt  | SQ1~P21 | Solution | 1         |      659,619 nodes at  164,414/s in 4 sec         | 
fr!    | bb:lock:sl:lt  | SQ1~P13 | TimeOut  |           |   62,283,880 nodes at  207,125/s in 5 min         | 
fr!    | bb:lock:sl:lt  | SQ1~P29 | TimeOut  |           |   59,125,106 nodes at  196,602/s in 5 min         | 
fr!    | bb:lock:sl:lt  | SQ1~P15 | Solution | 1         |      633,641 nodes at  207,291/s in 3 sec         | 
fr!    | bb:lock:sl:lt  | SQ1~P41 | TimeOut  |           |   36,355,271 nodes at  121,009/s in 5 min         | 
fr!    | bb:lock:sl:lt  | SQ1~P43 | TimeOut  |           |   61,136,432 nodes at  203,344/s in 5 min         | 
fr!    | bb:lock:sl:lt  | SQ1~P97 | TimeOut  |           |   52,908,325 nodes at  175,775/s in 5 min, 1 sec  | 
fr!    | bb:lock:sl:lt  | SQ1~P39 | TimeOut  |           |   59,331,065 nodes at  197,284/s in 5 min         | 
fr!    | bb:lock:sl:lt  | SQ1~P7  | TimeOut  |           |   61,322,930 nodes at  204,058/s in 5 min         | 
fr!    | bb:lock:bucket | SQ1~P27 | Solution | 3         |       12,935 nodes at   11,239/s in 1.2 sec       | 
fr!    | bb:lock:bucket | SQ1~P21 | Solution | 1         |      619,329 nodes at   10,781/s in 57 sec        | 
fr!    | bb:lock:bucket | SQ1~P13 | TimeOut  |           |    2,190,677 nodes at    7,295/s in 5 min         | 
fr!    | bb:lock:bucket | SQ1~P29 | TimeOut  |           |    2,210,794 nodes at    7,361/s in 5 min         | 
fr!    | bb:lock:bucket | SQ1~P15 | Solution | 1         |      559,001 nodes at   11,162/s in 50 sec        | 
fr!    | bb:lock:bucket | SQ1~P41 | TimeOut  |           |    1,366,217 nodes at    4,550/s in 5 min         | 
fr!    | bb:lock:bucket | SQ1~P43 | TimeOut  |           |    2,173,480 nodes at    7,237/s in 5 min         | 
fr!    | bb:lock:bucket | SQ1~P97 | TimeOut  |           |    2,172,571 nodes at    7,232/s in 5 min         | 
fr!    | bb:lock:bucket | SQ1~P39 | Solution | 1         |    1,720,984 nodes at    8,132/s in 3 min, 31 sec | 
fr!    | bb:lock:bucket | SQ1~P7  | TimeOut  |           |    2,179,976 nodes at    7,258/s in 5 min         | 
```

## Library

The original code used a SQL database for most internal testing. This has been removed, as I don't want any external dependencies.
