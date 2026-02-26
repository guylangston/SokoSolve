namespace SokoSolve.Reporting;

public readonly struct Cell
{
    public required string TextOnly { get; init; }
    public string? TextEscaped { get; init; }

    public int Length => TextOnly.Length;

    public static implicit operator Cell(string? txt)
    {
        return new Cell { TextOnly = txt ?? "" };
    }
}

