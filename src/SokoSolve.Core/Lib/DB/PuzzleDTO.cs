using System;
using System.Collections.Generic;

namespace SokoSolve.Core.Lib.DB
{
    public class PuzzleDTO
    {
        public int      PuzzleId { get; set; }
        public string?   Name     { get; set; }
        public int      Hash     { get; set; }
        public int      Rating   { get; set; }
        public string?   CharMap  { get; set; }
        public DateTime Created  { get; set; }
        public DateTime Modified { get; set; }

        public string? Author { get; set; }
        public string? Url    { get; set; }
        public string? Email  { get; set; }

        public string? SourceIdent { get; set; }

        public List<SolutionDTO>? Solutions { get; set; }
    }
}