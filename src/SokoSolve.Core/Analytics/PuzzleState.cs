using System.Text;

namespace SokoSolve.Core.Analytics
{
    public class PuzzleState
    {
        public StaticMaps Static { get; set; }
        public StateMaps Current { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Walls:");
            sb.Append(Static.WallMap);

            sb.AppendLine("Floors:");
            sb.Append(Static.FloorMap);

            sb.AppendLine("Goal:");
            sb.Append(Static.GoalMap);

            sb.AppendLine("CrateMap:");
            sb.Append(Current.CrateMap);

            sb.AppendLine("MoveMap:");
            sb.Append(Current.MoveMap);

            return sb.ToString();
        }
    }
}