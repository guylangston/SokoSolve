# SokoSolve
Sokoban puzzle game and solver

![.NET Core](https://github.com/guylangston/SokoSolve/workflows/.NET%20Core/badge.svg)

The origonal project was started and hosted at http://sokosolve.sourceforge.net/, code: https://sourceforge.net/projects/sokosolve/

It was moved to github at a much later date and the C# code updated, however the GUI client has not been ported over.

## Getting Started


```
C:\Projects\> git clone https://github.com/guylangston/SokoSolve.git
C:\Projects\SokoSolve\src\SokoSolve.Tests\> dotnet build
C:\Projects\SokoSolve\src\SokoSolve.Tests\> dotnet test
```

## Game

I have not ported over the GUI or Console game client from the old source-forge repo. There are NO current plans to re-implement a user interface.

If you want the user-interface, use the old version: http://sokosolve.sourceforge.net


## Solver

```cmd
C:\Projects\SokoSolve\src\SokoSolve.Console\> dotnet -c Release run
```
![Solver Progress](SolveRun2019-09-30-181744.png "Progress")

## Library

The origonal code used a SQL database for most internal testing. This has been removed, as I don't want any external dependancies.
