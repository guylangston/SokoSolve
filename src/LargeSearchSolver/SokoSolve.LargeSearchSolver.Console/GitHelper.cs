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
               RedirectStandardError  = true,
               UseShellExecute        = false,
            }
        };
        proc.Start();
        var stdout = new List<string>();
        var stderr = new List<string>();
        Task.WaitAll( ReadStdOut(), ReadStdErr());
        return [..stderr, ..stdout];

        async Task ReadStdErr()
        {
            while(await proc.StandardError.ReadLineAsync() is {} line)
            {
                stderr.Add(line);
            }
        }
        async Task ReadStdOut()
        {
            while(await proc.StandardOutput.ReadLineAsync() is {} line)
            {
                stdout.Add(line);
            }
        }
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
            if (ln.StartsWith("fatal: not a git repository")) return;
            await outp.WriteLineAsync($"GIT-LOG1: {ln}");
        }
        string[] skipWhen =
        [
            "On branch master",
            "Your branch is up to date with 'origin/master'.",
            "nothing to commit, working tree clean",
        ];
        List<string> status = new();
        foreach(var ln in await RunYieldingStdOutAsList(bin, "status"))
        {
            if (string.IsNullOrWhiteSpace(ln)) continue;
            if (ln.TrimStart().StartsWith('(')) continue;
            status.Add(ln);
        }
        if (status.Count > 3 || !status.SequenceEqual(skipWhen))
        {
            foreach(var ln in status)
            {
                await outp.WriteLineAsync($"GIT-STAT: {ln}");
            }
        }
    }
}


