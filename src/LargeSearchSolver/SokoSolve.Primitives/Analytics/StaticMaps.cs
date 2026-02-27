
namespace SokoSolve.Primitives.Analytics;

public class StaticMaps
{
    public  StaticMaps(Bitmap wallMap, Bitmap floorMap, Bitmap goalMap, Bitmap crateStart)
    {
        WallMap    = wallMap;
        FloorMap   = floorMap;
        GoalMap    = goalMap;
        CrateStart = crateStart;
    }

    public Bitmap WallMap    { get;  }
    public Bitmap FloorMap   { get;  }
    public Bitmap GoalMap    { get;  }
    public Bitmap CrateStart { get; }

}
