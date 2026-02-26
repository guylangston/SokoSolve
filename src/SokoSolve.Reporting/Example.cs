namespace SokoSolve.Reporting;

#if DEBUG
public static class Example
{
    public static void GenerateBasic(TextWriter tw)
    {
        var report = new CReport(tw);
        report.WriteLine("");
        report.Write("Hello ");
        report.Write("World");
        report.WriteLine();

    }
}

#endif
