using System.Runtime.CompilerServices;
using SkiaSharp;
using Svg.Skia;

namespace SokoSolve.UI.SkiaUI.Tests;

public abstract class TestAssetHelper
{
    readonly string projFile ="SokoSolve.UI.SkiaUI.Tests.csproj";
    string? projRoot;

    public string GetPathOutputFileName(string fileName)
    {
        var outAssets = "test-output";
        if (!Directory.Exists(Path.Combine(GetProjectRoot(), outAssets)))
        {
            Directory.CreateDirectory(Path.Combine(GetProjectRoot(), outAssets));
        }
        return Path.Combine(GetProjectRoot(), outAssets, fileName);
    }

    public string GetPathOutputUsingCallerName(string ext, [CallerMemberName] string caller = "")
        => GetPathOutputFileName(caller + ext);

    public string GetAssetPath(string file)
    {
        var outAssets = "assets";
        return Path.Combine(GetProjectRoot(), outAssets, file);
    }

    public string GetProjectRoot()
    {
        if (projRoot != null) return projRoot;
        if (File.Exists(projFile)) return "./";
        int cc = 0;
        while (cc < 5)
        {
            var pp = string.Concat(Enumerable.Range(0, cc).Select(x=>"../"));
            var p = pp + projFile;
            if (File.Exists(p))
            {
                projRoot = pp;
                return pp;
            }
            cc++;
        }
        throw new Exception($"Cannot find project file: {projFile}");
    }

    public SKPicture LoadSvg(string file)
    {
        var svg = new SKSvg();
        svg.Load(GetAssetPath(file));
        return svg.Picture ?? throw new Exception($"Invalid svg: {file}");
    }
}

