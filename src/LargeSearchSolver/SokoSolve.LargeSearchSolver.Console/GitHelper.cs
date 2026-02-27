using System.Diagnostics;
using SokoSolve.LargeSearchSolver.Utils;

namespace SokoSolve.LargeSearchSolver.Console;

public static class GitHelper
{
    // See: /home/guy/repo/Rustlings/src/GL.Helpers/ProcessHelper.cs
    public static async Task<List<string>> RunYieldingStdOutAsList(string prog, string args, string? directory = null)
    {
        using var proc = new Process
        {
            StartInfo = new ProcessStartInfo(prog, args)
            {
               WorkingDirectory       = directory,
               RedirectStandardOutput = true,
               UseShellExecute        = false,
            }
        };
        proc.Start();
        var res = new List<string>();
        while(await proc.StandardOutput.ReadLineAsync() is {} line)
        {
            res.Add(line);
        }
        return res;
    }

    public static async Task WriteGitStatus(TextWriter outp)
    {
        if (File.Exists("/usr/bin/git"))
        {
            await WriteGitStatus("/usr/bin/git", outp);
        }
        else
        {
            if (OSHelper.TryFindInEnvironmentPath(OSHelper.IsWindows() ? "git.exe" : "git", out var gitPath))
            {
                await WriteGitStatus(gitPath, outp);
            }
            else
            {
                outp.WriteLine("git not found");
            }
        }
    }

    public static async Task WriteGitStatus(string bin, TextWriter outp)
    {
        foreach(var ln in await RunYieldingStdOutAsList(bin, "log -1"))
        {
            if (string.IsNullOrWhiteSpace(ln)) continue;
            await outp.WriteLineAsync($"GIT-LOG1: {ln}");
        }
        foreach(var ln in await RunYieldingStdOutAsList(bin, "status"))
        {
            if (string.IsNullOrWhiteSpace(ln)) continue;
            if (ln.TrimStart().StartsWith('(')) continue;
            await outp.WriteLineAsync($"GIT-STAT: {ln}");
        }
    }
}




