# Progress Notes / Next Steps

> In time this should be deleted in favour of GitHub issues

2026-02-28: Added LNodeLookupSharding. Made at attempt at SQ1~P5 after 700mil nodes, crashed out of memory.
            There is not much headroom in improve memory usage, so an exhaustive tree search has little potential for improvement.
            I think LNodeStructEvaluatorForward is not very well optimised.
            The next step if LNodeStructEvaluatorReverse

# Next

- [X] Optimized low-gc `NodeStruct`
- [X] sharding NodeLookup
- [ ] ReverseEvalulator to mirror `LNodeEvalualtorForwardStable`
- [ ] Multi-threaded Coordinator

# Current

> dotnet run -c Release -- solve --puzzle __target --minRating 40 --maxTime 3600

# Done

- [X] Drop SokoSolve.Core.Solver (move to seperate project)
    - [X] OR: Drop SokoSolve.Core.Solver from LargeSearchSolver (then copy over the pieces still needed)
    - [X] Migrate core elements to `SokoSolve.Primitives`

