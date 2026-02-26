namespace SokoSolve.Primitives.Utils;

public static class OSHelper
{
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

    private static bool IsLinux()
    {
        return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
    }
}

