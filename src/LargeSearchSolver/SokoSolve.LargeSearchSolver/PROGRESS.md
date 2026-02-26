# Progress Notes / Next Steps

> In time this should be deleted in favour of GitHub issues

# Next

- [ ] ReverseEvalulator to mirror `LNodeEvalualtorForwardStable`
- [ ] Multi-threaded Coordinator

# Current

> dotnet run -c Release -- solve --puzzle __target --minRating 40 --maxTime 3600

# Done

- [X] Drop SokoSolve.Core.Solver (move to seperate project)
    - [X] OR: Drop SokoSolve.Core.Solver from LargeSearchSolver (then copy over the pieces still needed)
    - [X] Migrate core elements to `SokoSolve.Primitives`

