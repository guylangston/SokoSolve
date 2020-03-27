using System;
using SokoSolve.Core.Lib;

namespace SokoSolve.Game
{
    public class Profile
    {
        public string       Name       { get; set; }
        public DateTime     Created    { get; set; }
        public TimeSpan     TimeInGame { get; set; }
        public PuzzleIdent? Current    { get; set; }
        public string       FileName   { get; set; }

        public override string ToString() 
            => $"Name: {Name}, Created: {Created}, TimeInGame: {TimeInGame}, Current: {Current}";
    }
}