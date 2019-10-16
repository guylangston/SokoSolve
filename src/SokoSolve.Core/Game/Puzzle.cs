using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Library;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Game
{
   
    public interface IPosition
    {
        VectorInt2 Position { get; set; }
    }

   

    public class Puzzle : Puzzle<CellDefinition<char>, char>
    {
        private Puzzle(List<List<CellDefinition<char>>> map, CellDefinition<char>.Set definition) : base(map, definition)
        {
        }

        public Puzzle Clone()
        {
            var c = new List<List<CellDefinition<char>>>();
            foreach (var line in base.map)
            {
                c.Add(new List<CellDefinition<char>>(line));
            }

            return new Puzzle(c, Definition);
        }
        

        public static class Builder
        {
            
            public static Puzzle FromLines(IEnumerable<string> puzzleStr, CharCellDefinition.Set defn = null)
            {
                defn ??= CharCellDefinition.Default;
                return new Puzzle(
                    puzzleStr.Select(line => line.Select(c=>defn.Get(c)).ToList()).ToList(),
                    CharCellDefinition.Default);
            }


            public static Puzzle DefaultTestPuzzle() => FromLines(TestLibrary.DefaultPuzzleTest, CharCellDefinition.Default);


//            public Puzzle(Puzzle puzzle)
//            {
//                map = new List<List<char>>(puzzle.map.Select(x => new List<char>(x)));
//                Definition = puzzle.Definition;
//            }

            public static Puzzle FromMultLine(string puzzleText)
                => FromLines(
                    puzzleText.Split('\n').Select(x => x.Trim('\r')).Where(x => x.Length > 0),
                    CharCellDefinition.Default);

            public static Puzzle CreateEmpty()
            {
                return FromLines(new[]
                {
                        "####",
                        "#..#",
                        "#..#",
                        "####",
                });
            }
        }
    }

    public abstract class Puzzle<T, TCell> : IEnumerable<Puzzle<T, TCell>.Tile> 
        where T : CellDefinition<TCell>
    {
        protected readonly List<List<T>> map;    // TODO: Use a better 2d map class, which allows resizing, etc?

        protected Puzzle(List<List<T>> map, CellDefinition<TCell>.Set definition)
        {
            this.map = map;
            Definition = definition;
        }

        public struct Tile : IPosition
        {
            public VectorInt2 Position { get; set; }
            public T Cell { get; set; }
        }
        public CellDefinition<TCell>.Set Definition { get; } 

        public T this[int x, int y]
        {
            get => map[y][x];
            set => map[y][x] = value;
        }

        public T this[VectorInt2 p]
        {
            get => map[p.Y][p.X];
            set => map[p.Y][p.X] = value;
        }

        public int Width => map[0].Count;
        public int Height => map.Count;

        // Additional metadata (should not be here; just for convience)
     
        public Tile Player
        {
            get
            {
                foreach (var c in this)
                    if (c.Cell.IsPlayer)
                        return c;
                throw new Exception("Player not found");
            }
        }

        public RectInt Area => new RectInt(new VectorInt2(Width, Height));

       


        public IEnumerator<Tile> GetEnumerator()
        {
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                yield return new Tile
                {
                    Position = new VectorInt2(x, y),
                    Cell = this[x, y]
                };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Contains(VectorInt2 v)
        {
            if (v.X < 0 || v.Y < 0) return false;
            if (v.X >= Width || v.Y >= Height) return false;
            return true;
        }

        public Bitmap ToMap(CellDefinition<TCell> c) 
            => Bitmap.Create(new VectorInt2(Width, Height), Where(x => x == c).Select(x => x.Position));
        
        public Bitmap ToMap(TCell c) 
            => Bitmap.Create(new VectorInt2(Width, Height), Where(x => x.Underlying.Equals(c)).Select(x => x.Position));

        public Bitmap ToMap(IReadOnlyCollection<CellDefinition<TCell>> any) 
            => Bitmap.Create(new VectorInt2(Width, Height), Where(x => any.Contains(x)).Select(x => x.Position));

        public Bitmap ToMap(params CellDefinition<TCell>[] any) 
            => Bitmap.Create(new VectorInt2(Width, Height), Where(x => any.Contains(x)).Select(x => x.Position));

        public Bitmap ToMap(params TCell[] any) 
            => Bitmap.Create(new VectorInt2(Width, Height), Where(x => any.Contains(x.Underlying)).Select(x => x.Position));

        public IEnumerable<Tile> Where(Func<T, bool> where)
        {
            foreach (var cell in this)
                if (where(cell.Cell))
                    yield return cell;
            
        }

        public bool IsSolved => Count(Definition.Crate) == 0;

        public int Count(CellDefinition<TCell> state)
        {
            var cc = 0;
            foreach (var c in this)
                if (c.Cell.Equals(state))
                    cc++;
            return cc;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++) sb.Append(this[x, y]);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public List<string> ToStringList()
        {
            var res = new List<string>();
            for (var y = 0; y < Height; y++)
            {
                var sb = new StringBuilder();
                for (var x = 0; x < Width; x++) sb.Append(this[x, y]);
                res.Add(sb.ToString());
            }

            return res;
        }
    }
}