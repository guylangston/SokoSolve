using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
    public static class DevHelper
    {
        public static string DescribeCPU()
        {
            if (System.IO.File.Exists("/proc/cpuinfo"))
            {
                foreach (var ln in System.IO.File.ReadLines("/proc/cpuinfo"))
                {
                    if (ln.StartsWith("model name"))
                    {
                        return ln.Remove(0, "model name".Length).Trim('\t', ' ', ':').Trim();
                    }
                }

                return "UNKNOWN";
            }

            try
            {
                var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0\");
                return key?.GetValue("ProcessorNameString").ToString().Trim() ?? "Not Found";
            }
            catch (Exception)
            {
            }
            
            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER").Trim();
        }

        private static string gitLabel = null;
        public static string GetGitLabel()
        {
            if (gitLabel == null)
            {
                if (File.Exists("git-label.txt"))
                {
                    gitLabel = System.IO.File.ReadAllText("git-label.txt").Trim(' ', '\n', '\r');
                    return gitLabel;
                }

                gitLabel = "NotFound";
            }

            return gitLabel;
        }

        public static string RuntimeEnvReport() =>
            new FluentStringBuilder(" ")
                .Append(Environment.MachineName).Sep()
                .Append($"'{DescribeCPU()}'").Sep()
                .Append($"OS:{Environment.OSVersion.ToString().Replace("Microsoft Windows NT", "WIN").Replace(" ", "")}").Sep()
                .Append($"dotnet:{Environment.Version}").Sep()
                .Append($"Threads:{Environment.ProcessorCount}").Sep()
                .If(IsRunningOnMono(), "MONO").Sep()
                .Append(Environment.Is64BitProcess ? "x64" : "x32").Sep()
                .Append(IsDebug() ? "DEBUG" : "RELEASE");

        public static bool IsRunningOnMono() => Type.GetType("Mono.Runtime") != null;

        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public static string FullDevelopmentContext() 
            => $"{RuntimeEnvReport()}\nGit: '{GetGitLabel()}' at {DateTime.Now:u}, v{SokoSolve.Core.SokoSolveApp.Version}";

        public static void WriteFullDevelopmentContext(TextWriter outp, IReadOnlyDictionary<string, string> extras)
        {
            outp.WriteLine($"Computer: {RuntimeEnvReport()}");
            outp.WriteLine($" Version: '{GetGitLabel()}' at {DateTime.Now:u}, v{SokoSolve.Core.SokoSolveApp.Version}");
            var len = "Computer".Length;
            foreach (var pair in extras)
            {
                outp.WriteLine($"{pair.Key.PadLeft(len)}: {pair.Value}");
            }
            
        }
    }
}