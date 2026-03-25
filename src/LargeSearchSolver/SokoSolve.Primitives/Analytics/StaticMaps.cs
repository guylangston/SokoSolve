
namespace SokoSolve.Primitives.Analytics;

public class StaticMaps
{
    public StaticMaps(IReadOnlyBitmap wallMap, IReadOnlyBitmap floorMap, IReadOnlyBitmap goalMap, IReadOnlyBitmap crateStart)
    {
        WallMap    = wallMap;
        FloorMap   = floorMap;
        GoalMap    = goalMap;
        CrateStart = crateStart;
    }

    public IReadOnlyBitmap WallMap    { get;  }
    public IReadOnlyBitmap FloorMap   { get;  }
    public IReadOnlyBitmap GoalMap    { get;  }
    public IReadOnlyBitmap CrateStart { get; }

}
