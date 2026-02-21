namespace SokoSolve.LargeSearchSolver.ConsoleHost;

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
}



