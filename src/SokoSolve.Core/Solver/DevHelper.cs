using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Win32;

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
                        return ln.Remove(0, "model name".Length).Trim('\t', ' ', ':');
                    }
                }

                return "UNKNOWN";
            }

            try
            {
                var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0\");
                return key?.GetValue("ProcessorNameString").ToString() ?? "Not Found";
            }
            catch (Exception)
            {
            }
            
            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
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

        public static string DescribeHostMachine() =>
            Environment.MachineName + " " + (Environment.Is64BitProcess ? "x64" : "x32");

        public static string RuntimeEnvReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.MachineName);
            sb.Append(" running RT:");
            sb.Append(Environment.Version);
            sb.Append(" OS:'");
            sb.Append(Environment.OSVersion.ToString().Replace("Microsoft Windows NT", "WIN"));
            sb.Append("'");
            sb.Append($" Threads:{Environment.ProcessorCount}");
            if (IsRunningOnMono()) sb.Append(" MONO");
            sb.Append(IsDebug() ? " DEBUG" : " RELEASE");
            sb.Append(" ");
            sb.Append(Environment.Is64BitProcess ? "x64" : "x32");
            sb.Append(" '");
            sb.Append(DescribeCPU());
            sb.Append("'");
            return sb.ToString();
        }
        
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
        {
            return $"{RuntimeEnvReport()}\nGit: '{GetGitLabel()}' at {DateTime.Now:u}";
        }
    }
}