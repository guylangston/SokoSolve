namespace SokoSolve.Reporting;

public class ReportTable : DeferredRendering
{
    readonly List<List<Cell>> table = [  ];

    public ReportTable Add(Cell c)
    {
        table.Last().Add(c);
        return this;
    }

    public ReportTable AddLine()
    {
        table.Add( new List<Cell>() );
        return this;
    }

    public ReportTable AddLine(params object?[] cells)
    {
        table.Add(cells.Select(x=>new Cell { TextOnly = x?.ToString() ?? "" }).ToList());
        return this;
    }

    public ReportTable AddLine(params Cell[] cells)
    {
        table.Add(cells.ToList());
        return this;
    }

    public override void WriteTo(CReport report)
    {
        // measure all cols
        var maxCols = new List<int>();
        foreach(var row in table)
        {
            for(var cc=0; cc<row.Count; cc++)
            {
                if (maxCols.Count <= cc) maxCols.Add(0);
                if (maxCols[cc] < row[cc].Length) maxCols[cc] = row[cc].Length;
            }
        }

        // draw
        foreach(var row in table)
        {
            for(var cc=0; cc<row.Count; cc++)
            {
                report.Write(row[cc].TextOnly);
                report.WritePadding(maxCols[cc] - row[cc].Length);
                report.Write(" | ");
            }
            report.WriteLine();
        }
    }
}


