namespace SokoSolve.Reporting;

public class LabelReport : DeferredRendering
{
    readonly List<(Cell Label, Cell Value)> labels = [];

    public LabelReport Add(Cell lbl, Cell val)
    {
        labels.Add((lbl, val));
        return this;
    }

    public override void WriteTo(CReport report)
    {
        // measure
        var maxLabel = labels.Max(x=>x.Label.Length);
        foreach(var item in labels)
        {
            report.Write(item.Label);
            report.Write(": ");
            report.WritePadding(maxLabel - item.Label.Length);
            report.Write(item.Value);
            report.WriteLine();
        }
    }
}

