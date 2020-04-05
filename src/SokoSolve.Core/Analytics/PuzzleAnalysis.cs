using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;

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
                    SolverHelper.FloodFillUsingWallAndCrates(Static.WallMap, crateMap,current.Player.Position))
            );
        }
    }
}