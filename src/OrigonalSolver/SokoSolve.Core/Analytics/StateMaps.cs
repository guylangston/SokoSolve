using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Analytics
{
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

        public override bool Equals(object obj)
        {
            var rhs = obj as StateMaps;
            if (rhs == null) return false;
            return rhs.CrateMap.Equals(CrateMap) && rhs.MoveMap.Equals(MoveMap);
        }

        public override string ToString()
        {
            return $"CrateMap:\n{CrateMap}\nMoveMap:\n{MoveMap}";
        }
    }
}