using SokoSolve.Core.Primitives;
using SokoSolve.Core.PuzzleLogic;

namespace SokoSolve.Core.Analytics
{
    public class PuzzleAnalysis
    {
        public PuzzleAnalysis(Puzzle start)
        {
            Start = start;
            Static = StaticAnalysis.Generate(start);

            Static.DeadMap = DeadMapAnalysis.FindDeadMap(Static);
        }

        public StaticMaps Static { get; protected set; }

        public Puzzle Start { get; set; }

        public PuzzleState Evalute(Puzzle current)
        {
            var crateMap = current.ToMap(current.Definition.AllCrates);
            return new PuzzleState
            {
                Static = Static,
                Current = new StateMaps
                {
                    CrateMap = crateMap,
                    MoveMap = FloodFill.Fill(Static.WallMap.BitwiseOR(crateMap), current.Player.Position)
                }
            };
        }
    }
}