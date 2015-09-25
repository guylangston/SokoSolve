using Sokoban.Core.PuzzleLogic;

namespace Sokoban.Core.Library
{
    public static class PuzzleHelper
    {
        public static string GetName(Puzzle puzzle)
        {
            if (!string.IsNullOrWhiteSpace(puzzle.Name)) return puzzle.Name;
            return string.Format("Unnamed {0}x{1}", puzzle.Width, puzzle.Height);
        }
    }
}