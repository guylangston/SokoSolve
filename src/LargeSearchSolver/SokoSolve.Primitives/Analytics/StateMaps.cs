namespace SokoSolve.Primitives.Analytics;

public interface IStateMaps
{
    IBitmap CrateMap { get; }
    IBitmap MoveMap { get; }
}

public class StateMaps : IStateMaps
{
    public StateMaps(Bitmap crateMap, Bitmap moveMap)
    {
        CrateMap = crateMap;
        MoveMap = moveMap;
    }

    public IBitmap CrateMap { get;  }
    public IBitmap MoveMap { get;  }

    public static StateMaps Create(Puzzle puzzle) =>
        new StateMaps(puzzle.ToMap(puzzle.Definition.AllCrates),
            FloodFill.Fill(puzzle.ToMap(puzzle.Definition.Wall), puzzle.Player.Position));

    public override int GetHashCode()
    {
        return CrateMap.GetHashCode() ^ MoveMap.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if(obj is StateMaps rhs)
        {
            return rhs.CrateMap.Equals(CrateMap) && rhs.MoveMap.Equals(MoveMap);
        }
        return base.Equals(obj);
    }

    public override string ToString()
    {
        return $"CrateMap:\n{CrateMap}\nMoveMap:\n{MoveMap}";
    }
}
