using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SokoSolve.Core.Library;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.PuzzleLogic
{

    public interface IPosition
    {
        VectorInt2 Position { get; set; }
    }

    public struct Cell : IPosition
    {
        public VectorInt2 Position { get; set; }
        public Char State { get; set; }
    }


    public class Puzzle  : IEnumerable<Cell>
    {
        private readonly List<List<char>> map;

        public CellDefinition Definition = CellDefinition.Default;

        public Puzzle() : this(TestLibrary.Default)
        {
        }

        public Puzzle(IEnumerable<string> puzzleStr)
        {
            map = puzzleStr.Select(x => new List<char>(x)).ToList();
        }

        public Puzzle(Puzzle puzzle)
        {
            map = new List<List<char>>(puzzle.map.Select(x=>new List<char>(x)));
            Definition = puzzle.Definition;
        }

        public Puzzle(string puzzleText) : this(puzzleText.Split('\n').Select(x=>x.Trim('\r')).Where(x=>x.Length > 0))
        {
            
        }

        public Puzzle Clone()
        {
            return new Puzzle(this);
        }

        public char this[int x, int y]
        {
            get { return map[y][x]; }
            set { map[y][x] = value; }
        }

        public char this[VectorInt2 p]
        {
            get { return map[p.Y][p.X]; }
            set { map[p.Y][p.X] = value; }
        }

        public int Width { get { return map[0].Count; } }
        public int Height { get { return map.Count; } }

        // Additional metadata (should not be here; just for convience)

        public string Name { get; set; }
        public object Tag { get; set; }


        public bool Contains(VectorInt2 v)
        {
            if (v.X < 0 || v.Y < 0) return false;
            if (v.X >= Width || v.Y >= Height) return false;
            return true;
        }

        public Bitmap ToMap(char c)
        {
            return Bitmap.Create(new VectorInt2(Width, Height), Where(x=>x == c).Select(x=>x.Position));
        }

        public Bitmap ToMap(params char[] c)
        {
            return Bitmap.Create(new VectorInt2(Width, Height), Where(x => c.Contains(x)).Select(x => x.Position));
        }

        public IEnumerable<Cell> Where(Func<char, bool> where)
        {
            foreach (var cell in this)
            {
                if (where(cell.State)) yield return cell;
            }
        }

        public Cell Player
        {
            get
            {
                foreach (var c in this)
                {
                    if (Definition.IsPlayer(c.State))
                    {
                        return c;
                    }
                }
                return new Cell()
                {
                    Position = new VectorInt2(-1, -1)
                };
            }
        }

        public RectInt2 Area
        {
            get
            {
                return new RectInt2()
                {
                    Size = new VectorInt2(Width, Height)
                };
            }
        }

        public bool IsSolved
        {
            get { return Count(Definition.Crate) == 0; }
        }

        

        public int Count(char state)
        {
            int cc = 0;
            foreach (var c in this)
            {
                if (c.State == state) cc++;
            }
            return cc;
        }


        public IEnumerator<Cell> GetEnumerator()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return new Cell()
                    {
                        Position = new VectorInt2(x, y),
                        State = this[x, y]
                    };
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    sb.Append(this[x, y]);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public List<string> ToStringList()
        {
            var res = new List<string>();
            for (int y = 0; y < Height; y++)
            {
                var sb = new StringBuilder();
                for (int x = 0; x < Width; x++)
                {
                    sb.Append(this[x, y]);
                }
                res.Add(sb.ToString());
            }
            return res;
        }
    }
}