using VectorInt;

namespace SokoSolve.Primitives.Analytics;

public class PuzzleAnalysis
{
    public PuzzleAnalysis(Puzzle start)
    {
        Start = start;
        Static = new  StaticAnalysisMaps(start);
    }

    public StaticAnalysisMaps Static { get; protected set; }
    public Puzzle     Start  { get; set; }

    public PuzzleState Evalute(Puzzle current)
    {
        var crateMap = current.ToMap(current.Definition.AllCrates);
        return new PuzzleState(Static,
                new StateMaps(crateMap,
                FloodFillUsingWallAndCrates(Static.WallMap, crateMap,current.Player.Position))
        );
    }

    public static Bitmap FloodFillUsingWallAndCrates(IBitmap wall, IBitmap crate, VectorInt2 pp)
    {
        var fillConstraints = new BitmapSpan(wall.Size, stackalloc uint[wall.Height]);
        fillConstraints.SetBitwiseOR(wall, crate);

        return FloodFill.Fill(fillConstraints, pp);
    }
}
