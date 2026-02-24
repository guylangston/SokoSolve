using System;

namespace SokoSolve.Primitives;


public class CharCellDefinition : CellDefinition<char>
{
    private CharCellDefinition(char underlying, Set memberOf) : base(underlying, memberOf)
    {
    }

    public static readonly Set Default = new Set('~', '#', '.', 'O', 'X',  '$', 'P', '*');

}
