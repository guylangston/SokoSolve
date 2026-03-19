using System.Security;
using System.Text;

namespace SokoSolve.LargeSearchSolver.Utils;

public static class GeneralHelper
{
    public static BuildFlagsState BuildFlags() => new BuildFlagsState();

    public class BuildFlagsState
    {
        StringBuilder sb = new();

        public string Seperator { get; set; } = ",";
        public string SeperatorLabel { get; set; } = ":";

        public BuildFlagsState AddFlag(string flag)
        {
            if (sb.Length > 0) sb.Append(Seperator);
            sb.Append(flag);
            return this;
        }

        public BuildFlagsState AddIf(bool ifTrue, string flag)
        {
            if (ifTrue)
            {
                return AddFlag(flag);
            }
            return this;
        }

        public BuildFlagsState AddObj(object? obj) => AddIf(obj != null, obj?.ToString() ?? "");
        public BuildFlagsState AddLabel(string label, object? obj) => AddIf(obj != null, $"{label}{SeperatorLabel}{obj}");

        public override string ToString() => sb.ToString();
    }
}



