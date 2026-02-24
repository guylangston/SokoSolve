# Progress Notes / Next Steps

> In time this should be deleted in favour of GitHub issues

# Next
- [ ] Drop SokoSolve.Core.Solver (move to seperate project)
    - [ ] OR: Drop SokoSolve.Core.Solver from LargeSearchSolver (then copy over the pieces still needed)

    ```csharp
    # Looks this is all that is used

    // TEST: Dependency to SokoSolve.Core?
    public interface IBitmap {}
    public class StaticAnalysisMaps { }
    public class Puzzle { }
    ```
