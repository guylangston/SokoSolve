namespace SokoSolve.Reporting;



public class CReport
{
    readonly TextWriter outp;

    public CReport(TextWriter outp)
    {
        this.outp = outp;
    }

    public bool AllowEscaping { get; set; }

    public CReport Write(char txt)     { outp.Write(txt); return this; }
    public CReport Write(string txt)     { outp.Write(txt); return this; }
    public CReport WriteLine(string txt) { outp.WriteLine(txt); return this; }
    public CReport WriteLine()           { outp.WriteLine(); return this; }


    public CReport Write(Cell cell)     { outp.Write(AllowEscaping ? (cell.TextEscaped ?? cell.TextOnly) : cell.TextOnly ); return this; }
    public CReport WritePadding(int size)
    {
        if (size <= 0) return this;
        for(int cc=0; cc<size; cc++)
            Write(' ');
        return this;
    }

    public CReport WriteRaw(Action<TextWriter> raw)
    {
        raw(outp);
        return this;
    }

    public CReport WriteLabels(Action<ReportLabel> labels)
    {
        var l = new ReportLabel();
        labels(l);
        l.WriteTo(this);
        return this;
    }

    public CReport WriteTable(Action<ReportTable> tbl)
    {
        var l = new ReportTable();
        tbl(l);
        l.WriteTo(this);
        return this;
    }


}

