namespace SokoSolve.Primitives;

public class PathHelper
{
    public string GetDataPath()
    {
        var cp = Environment.CurrentDirectory.Replace('\\', '/');

        var idx = cp.IndexOf("/SokoSolve/", StringComparison.Ordinal);
        if (idx > 0)
        {
            return cp[0..(idx + "/SokoSolve/".Length)] + "data";
        }

        if (Directory.Exists("./data")) return "./data/";
        if (Directory.Exists("../../data")) return "../../data/";

        throw new Exception($"Unable to find 'data' path. Curr={Environment.CurrentDirectory}");
    }

    public string GetLibraryPath() => System.IO.Path.Combine(GetDataPath(), "Lib");
    public string GetRelDataPath(string rel) => System.IO.Path.Combine(GetDataPath(), rel);
}
