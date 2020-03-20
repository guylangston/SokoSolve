using System;
using System.Collections.Generic;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Game;

namespace SokoSolve.Core.Lib
{
    public class Library : List<LibraryPuzzle>
    {
        public AuthoredItem Details { get; set; }

        public LibraryPuzzle this[string name]
        {
            get { return Find(x => x.Details != null && x.Details.Name == name); }
        }

        public int IndexOf(string name)
        {
            return FindIndex(x => x.Details != null && x.Details.Name == name);
        }

        public LibraryPuzzle GetNext(string name)
        {
            var idx = IndexOf(name);
            if (idx < 0) throw new Exception("Not Found:" + name);
            if (idx >= Count) return null;
            return this[idx + 1];
        }

        public LibraryPuzzle GetPrev(string name)
        {
            var idx = IndexOf(name);
            if (idx < 0) throw new Exception("Not Found:" + name);
            if (idx == 0) return null;
            return this[idx - 1];
        }
    }

    public class AuthoredItem
    {
        public string Name        { get; set; }
        public string Description { get; set; }
        public string Email       { get; set; }
        public string Url         { get; set; }
        public string Author      { get; set; }
        public string License     { get; set; }
        public string Tags        { get; set; } // Comma-separated list 
    }


    public class LibraryPuzzle 
    {
        public LibraryPuzzle()
        {
            Details = new AuthoredItem();
            Puzzle = Puzzle.Builder.CreateEmpty();
        }

        public LibraryPuzzle(Puzzle puzzle)
        {
            Puzzle = puzzle;
        }

        public PuzzleIdent  Ident    { get; set; }
        public AuthoredItem Details { get; set; }
        public Puzzle       Puzzle   { get; }
        public string       Name     { get; set; }
        public object       Tag      { get; set; }
        public double       Rating   { get; set; }
        public Path         Solution { get; set; }
    }

    /// <summary>
    ///     Library Collection (just a list of filenames)
    /// </summary>
    public class Collection
    {
        /// <summary>
        ///     Orderd list of files
        /// </summary>
        public List<string> Libraries { get; set; }

        public Dictionary<string, AuthoredItem> DetailLookup { get; set; }
    }

    public class PuzzleIdent
    {
        public string Library { get; set; }
        public string Puzzle { get; set; }

        public override string ToString()
        {
            return $"{Library};{Puzzle}";
        }

        public static PuzzleIdent Parse(string value)
        {
            var split = value.Split(';');
            return new PuzzleIdent
            {
                Library = split[0],
                Puzzle = split[1]
            };
        }
    }

    public class Profile
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }

        public TimeSpan TimeInGame { get; set; }

        public PuzzleIdent Current { get; set; }

        public Statistics Statistics { get; set; }
        public string FileName { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, Created: {1}, TimeInGame: {2}, Current: {3}, AllTimeStatistics: {4}", Name,
                Created, TimeInGame, Current, Statistics);
        }
    }
}