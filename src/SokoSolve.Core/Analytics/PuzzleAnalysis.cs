using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Analytics
{
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
                    FloodFill.Fill(Static.WallMap.BitwiseOR(crateMap), current.Player.Position))
            );
        }
    }
}