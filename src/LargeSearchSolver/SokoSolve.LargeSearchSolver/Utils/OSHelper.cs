using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SokoSolve.LargeSearchSolver.Utils;


public static class OSHelper
{
    [DllImport("libSystem.dylib")]
    private static extern int sysctlbyname(string name, IntPtr oldp, ref nint oldlenp, IntPtr newp, nint newlen);

    [StructLayout(LayoutKind.Sequential)]
    private struct XswUsage { public ulong Total, Avail, Used; public uint PageSize; public int Encrypted; }

    /// <summary>Max memory availble without too much swapping</summary>
    public static long GetAvailableMemory()
    {
        try
        {
            foreach (var line in File.ReadLines("/proc/meminfo"))
            {
                if (line.StartsWith("MemAvailable:"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return long.Parse(parts[1]) * 1024; // kB to bytes
                }
            }
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>Free memory available to a new process</summary>
    public static long GetMemoryFree()
    {
        try
        {
            foreach (var line in File.ReadLines("/proc/meminfo"))
            {
                if (line.StartsWith("MemFree:"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return long.Parse(parts[1]) * 1024; // kB to bytes
                }
            }
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    public static bool? UsingSwapMemory()
    {
        if (IsLinux())
        {
            foreach(var line in File.ReadLines("/proc/self/status"))
            {
                if (line.StartsWith("VmSwap:"))
                {
                    return !line.EndsWith(" 0 kB");
                }
            }
            throw new InvalidDataException("VmSwap not found, but expected");
        }
        if (IsMacOS())
        {
            nint size = Marshal.SizeOf<XswUsage>();
            var ptr = Marshal.AllocHGlobal((int)size);
            try
            {
                if (sysctlbyname("vm.swapusage", ptr, ref size, IntPtr.Zero, 0) == 0)
                    return Marshal.PtrToStructure<XswUsage>(ptr).Used > 0;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        return null;
    }

    public static async Task<List<string>> GetLinuxMemoryInfo()
    {
        if (IsLinux())
        {
            var filtered = new List<string>(5);
            await foreach(var ln in File.ReadLinesAsync("/proc/meminfo"))
            {
                if (ln.StartsWith("Mem") || ln.StartsWith("Swap"))
                    filtered.Add(ln);
            }
            return filtered;
        }

        return [];
    }

    public static bool IsLinux()   => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
    public static bool IsWindows() => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
    public static bool IsMacOS()   => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);
    public static bool TryFindInEnvironmentPath(string bin, [NotNullWhen(true)] out string? binPath)
    {
        var pathStr = Environment.GetEnvironmentVariable("PATH");
        if (pathStr == null)
        {
            binPath = null;
            return false;
        }

        // split path
        //
        foreach(var loc in pathStr.Split(Path.PathSeparator))
        {
            if (Path.Exists(loc))
            {
                var itemPath = Path.Combine(loc, bin);
                if (Path.Exists(itemPath))
                {
                    binPath = itemPath;
                    return true;
                }
            }
        }


        binPath = null;
        return false;
    }
}


