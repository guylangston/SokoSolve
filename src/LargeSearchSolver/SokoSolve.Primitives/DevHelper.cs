using System.Runtime.InteropServices;
using TextRenderZ;

namespace SokoSolve.Primitives;

public static class DevHelper
{
    public static string DescribeCPU()
    {
        if (File.Exists("/proc/cpuinfo"))
        {
            foreach (var ln in File.ReadLines("/proc/cpuinfo"))
            {
                if (ln.StartsWith("model name"))
                {
                    return ln.Remove(0, "model name".Length).Trim('\t', ' ', ':').Trim();
                }
            }

            return "UNKNOWN";
        }

        var procId = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
        return procId?.Trim() ?? "UNKNOWN";
    }

    private static string? gitLabel = null;
    public static string GetGitLabel()
    {
        if (gitLabel == null)
        {
            if (File.Exists("git-label.txt"))
            {
                gitLabel = File.ReadAllText("git-label.txt").Trim(' ', '\n', '\r');
                return gitLabel;
            }

            gitLabel = "NotFound";
        }

        return gitLabel;
    }

    public static string RuntimeEnvReport() =>
        new FluentString(" ")
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

    public static bool TryGetTotalMemory(out ulong avail)
    {
        if (File.Exists("/proc/meminfo"))
        {
            foreach (var ln in File.ReadLines("/proc/meminfo"))
            {
                if (ln.StartsWith("MemFree"))
                {
                    avail = ulong.Parse(ln.Remove(0, "MemFree".Length).Trim('\t', ' ', ':').Trim('k', 'B')) * 1024;
                    return true;
                }
            }
        }
        try
        {
            var memoryStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memoryStatus))
            {
                avail = memoryStatus.ullAvailPhys;
                return true;
            }
        }
        catch (Exception)
        {
            // ignored
        }

        avail = 0;
        return false;
    }

    [StructLayout(LayoutKind.Sequential, CharSet =CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
        public uint  dwLength;
        public uint  dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
        public MEMORYSTATUSEX()
        {
            this.dwLength = (uint) Marshal.SizeOf(typeof( MEMORYSTATUSEX ));
        }
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GlobalMemoryStatusEx( [In, Out] MEMORYSTATUSEX lpBuffer);

    public const long GiB_1 = 1024 * 1024 * 1024;
}
