# A Sokoban solver for very large search trees

## Goals

- Low GC Pressure
- Drop older tech-debt and experimental code
- Cleaner implementation, leasons-learned (but at the cost of some existing features / UI integration)

## Design Decisions

- First class NodeHeap with pooling
- Favour custom struct vs tradional class + GC
- Good, accurate documentation (before writing code)
- Standard Solver should be built for multi-threading
- Should support more complex SolverStrategy beyond Forward and Rervese, aka CratePushLines etc
- From Scratch (also zero-dependancy), I won't reused the existing primitives and solver infrastructure.
    > This is a serious trade-off, which is very risky and will cause a lots of rework. But should give a cleaner second-mover advantage
- Proof from unit tests (that is not really the case with the older code)

# Core Types and Names

- `NodeStruct` the fundemental search unit
- `NodeHeap` the only way to create a node, has both fwd and rev nodes
- `SolverCoordinator` start/stop/pause control the main loop. Must support cancellation and threads
- `SolverStrategy` fwd/rev/CrateMovePath, per TreeDepth, per Score, 1 thread per strategy
- `PrimaryBacklog` all unevaluated nodes (evaluated means all children are created and checked)


## Tree Mechanics
- There is one fwd tree and 1 reverse tree (both are sourced from the NodeHeap)
- Duplicate nodes are NOT added as children
- Adding children shound be a thread-safe transaction, so we don't add one at a time

```
    ALLOC  -->  UNEVAL   -->     EVAL    -->    COMMITTED
                                         -->    POOLED          --> ALLOC

    ALLOC   - in pool, a clean unused node, not yet leased
    UNEVAL  - leades node
    EVAL    - all children are generated
    COMMITTED - all children are commited to the tree and removed from backlog. Frozen/Immutable
    POOLED  - returned to pool, same as ALLOC
```

## What is the flow of a puzzle being solved?

```
    Puzzle -> SolverCommand --> SolverState (working state)
                                    --> SolverStateLocal (optionally local Backlog)
                                    --> SolverStateLocal
                                    --> SolverStateLocal
                                    --> SolverStateLocal
                                    ...

        SolverState
            .StaticAnalysis
            .NodeHeap

```
