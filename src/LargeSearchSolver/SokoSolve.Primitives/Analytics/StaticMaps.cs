
namespace SokoSolve.Primitives.Analytics;

public class StaticMaps
{
    public  StaticMaps(IBitmap wallMap, IBitmap floorMap, IBitmap goalMap, IBitmap crateStart)
    {
        WallMap    = wallMap;
        FloorMap   = floorMap;
        GoalMap    = goalMap;
        CrateStart = crateStart;
    }

    public IBitmap WallMap    { get;  }
    public IBitmap FloorMap   { get;  }
    public IBitmap GoalMap    { get;  }
    public IBitmap CrateStart { get; }

}
