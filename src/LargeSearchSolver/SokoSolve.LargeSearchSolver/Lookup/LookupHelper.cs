namespace SokoSolve.LargeSearchSolver.Lookup;

public static class LookupHelper
{
    public static (string Name, string? Desc) Descibe<T>(T item)
    {
        if (item is ISolverComponent sc)
        {
            return (sc.GetComponentName(), sc.Describe());
        }
        return (item?.GetType().Name ?? "(null)", null);
    }
}


