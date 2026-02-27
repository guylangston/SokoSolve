namespace SokoSolve.Primitives;

public static class Humanize
{
    public readonly struct Quanity
    {
        public required float Amount { get; init; }
        public string? Units { get; init; }
        public string? FormattedString { get; init; }

        public override string ToString()
        {
            if (FormattedString != null) return FormattedString;
            if (Units != null) return $"{Amount:#,##0.0}{Units}";
            return Amount.ToString("#,##0.0");
        }
    }

    public static Quanity TimeSpan(TimeSpan ts)
    {
        if (ts.TotalSeconds < 1)
            return new Quanity { Amount = (float)ts.TotalMilliseconds, Units = "ms" };
        if (ts.TotalMinutes < 1)
            return new Quanity { Amount = (float)ts.TotalSeconds, Units = "sec" };
        if (ts.TotalHours < 1)
            return new Quanity { Amount = (float)ts.TotalMinutes, Units = "min" };
        if (ts.TotalDays < 1)
            return new Quanity { Amount = (float)ts.TotalHours, Units = "hr" };
        return new Quanity { Amount = (float)ts.TotalDays, Units = "days" };
    }

    public static Quanity Bytes(long totalBytes)
    {
        if (totalBytes < 1024)
            return new Quanity { Amount = totalBytes, Units = "B" };
        if (totalBytes < 1024 * 1024)
            return new Quanity { Amount = (float)totalBytes / 1024, Units = "KB" };
        if (totalBytes < 1024 * 1024 * 1024)
            return new Quanity { Amount = (float)totalBytes / (1024 * 1024), Units = "MB" };
        if (totalBytes < 1024L * 1024 * 1024 * 1024)
            return new Quanity { Amount = (float)totalBytes / (1024 * 1024 * 1024), Units = "GB" };
        return new Quanity { Amount = (float)totalBytes / (1024L * 1024 * 1024 * 1024), Units = "TB" };
    }

}

