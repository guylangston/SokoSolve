namespace SokoSolve.Primitives;

public static class StaticAnalysis
{
    public static double CalculateRating(SokobanPuzzle puzzle)
    {
        var floors = puzzle.Definition.AllFloors.Sum(x => puzzle.Count(x));
        var crates = puzzle.Count(puzzle.Definition.Crate) + puzzle.Count(puzzle.Definition.CrateGoal);
        return floors + Math.Pow(crates, 3);
    }

    public static double CalculateRating2(SokobanPuzzle puzzle)
    {
        var floors = puzzle.Definition.AllFloors.Sum(x => puzzle.Count(x));
        var crates = puzzle.Count(puzzle.Definition.Crate) + puzzle.Count(puzzle.Definition.CrateGoal);

        return Math.Round(Math.Sqrt((floors - crates) * Math.Pow(crates, 2)));
    }
}
