using System.Text;

namespace SokoSolve.Core.Analytics
{
    public struct PuzzleState
    {
        public PuzzleState(StaticMaps @static, StateMaps current)
        {
            Static = @static;
            Current = current;
        }

        public StaticMaps Static { get;  }
        public StateMaps Current { get;  }

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