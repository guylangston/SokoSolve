﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Lib
{
    public class Library : List<LibraryPuzzle>
    {
        public AuthoredItem? Details { get; set; }

        public LibraryPuzzle this[string name]
        {
            get { return Find(x => x.Details != null && x.Details.Name == name); }
        }

        public int IndexOf(string name)
        {
            return FindIndex(x => x.Details != null && x.Details.Name == name);
        }

        public LibraryPuzzle? GetNext(string name)
        {
            var idx = IndexOf(name);
            if (idx < 0) throw new Exception("Not Found:" + name);
            if (idx >= Count) return null;
            return this[idx + 1];
        }

        public LibraryPuzzle? GetPrev(string name)
        {
            var idx = IndexOf(name);
            if (idx < 0) throw new Exception("Not Found:" + name);
            if (idx == 0) return null;
            return this[idx - 1];
        }
    }

    public class AuthoredItem
    {
        public string? Id          { get; set; }
        public string? Name        { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public string? Url         { get; set; }
        public string? Author      { get; set; }
        public string? License     { get; set; }
        public string? Tags        { get; set; } // Comma-separated list 
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

        public PuzzleIdent   Ident    { get; set; }
        public Puzzle        Puzzle   { get; }
        public AuthoredItem? Details  { get; set; }
        public string        Name     { get; set; }
        public object?       Tag      { get; set; }
        public double        Rating   { get; set; }
        public Path?         Solution { get; set; }
    }

    
    public class LibraryCollection
    {
        public List<LibrarySummary>? Items { get; set; }

        public string IdToFileName(string id) => Items?.FirstOrDefault(x => x.Id == id)?.FileName  ?? throw new ArgumentException($"NotFound: {id}.");

    }

    public class LibrarySummary
    {
        public string? Id       { get; set; }
        public string? Name     { get; set; }
        public string? FileName { get; set; }
    }

    public class PuzzleIdent
    {
        public string Library { get; set; }
        public string Puzzle { get; set; }

        public PuzzleIdent(string library, string puzzle)
        {
            Library = library;
            Puzzle = puzzle;
        }

        public override string ToString() => $"{Library}~{Puzzle}";

        public static PuzzleIdent Parse(string value)
        {
            var split = value.Split('~');
            if (split.Length != 2) throw new InvalidDataException(value);
            return new PuzzleIdent(split[0], split[1]);
        }

        private static int tempId = 100;
        public static PuzzleIdent Temp() => new PuzzleIdent("TMP", $"T{Interlocked.Increment(ref tempId)}");
    }

   
}