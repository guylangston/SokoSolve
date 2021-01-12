using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Primitives;
using VectorInt;
using VectorInt.Collections;

namespace SokoSolve.Core
{
    public class Puzzle : Puzzle<char>
    {
        private Puzzle(IReadOnlyCartesianMap<CellDefinition<char>> map, CellDefinition<char>.Set definition) : base(map, definition)
        {
        }

        public Puzzle Clone() => new Puzzle(this, Definition);

        public static class Builder
        {
            public static Puzzle FromLines(IEnumerable<string> puzzleStr, CharCellDefinition.Set? defn = null)
            {
                defn ??= CharCellDefinition.Default;
                return new Puzzle( 
                    CartesianMapBuilder.Create(puzzleStr.Select(line => line.Select(c=>defn.Get(c)).ToList()).ToList()),
                    CharCellDefinition.Default);
            }


            public static Puzzle DefaultTestPuzzle() => FromLines(TestLibrary.DefaultPuzzleTest, CharCellDefinition.Default);
            
            
            public static Puzzle Reference_UnSolved_SQ1P13() => Puzzle.Builder.FromLines(new[] {
                "~~~~~~~~~~~######~~",
                "~~~~####~~##....#~~",
                "~~###..#~~#..##.###",
                "###....####.#...X.#",
                "#..X.P.OOO$OO..X..#",
                "#.X.X..##.###...###",
                "###.###...#~#####~~",
                "~#......###~~~~~~~~",
                "~#...####~~~~~~~~~~",
                "~#####~~~~~~~~~~~~~",
            });
            public static Puzzle Reference_Solved_SQ1P15() => Puzzle.Builder.FromLines(new[] {
                "~~~~#######~~",
                "~~~~#.....##~",
                "#####.###..##",
                "#.......#..##",
                "#PX$$$O.##X.#",
                "#..#....##.O#",
                "##..##..#.X.#",
                "~##..####OXO#",
                "~~##........#",
                "~~~######..##",
                "~~~~~~~~####~",
            });
            
            public static Puzzle SQ1_P5() => Puzzle.Builder.FromLines(new[] {
                "~~~~~~~~~~~#####",
                "~~~~~~~~~~##...#",
                "~~~~~~~~~~#....#",
                "~~~~####~~#.X.##",
                "~~~~#..####X.X#~",
                "~~~~#.....X.X.#~",
                "~~~##.##.X.X.X#~",
                "~~~#..O#..X.X.#~",
                "~~~#..O#......#~",
                "#####.#########~",
                "#OOOO.P..#~~~~~~",
                "#OOOO....#~~~~~~",
                "##..######~~~~~~",
                "~####~~~~~~~~~~~",
            });


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
    
    

    public abstract class Puzzle<T> :  CartesianMap<CellDefinition<T>>
    {
        protected Puzzle(IReadOnlyCartesianMap<CellDefinition<T>> map, CellDefinition<T>.Set definition) : base(map)
        {
            Definition = definition;
            foreach (var cell in this.ForEach())
            {
                if (cell.Value is null)
                {
                    this[cell.Position] = definition.Void;
                }
            }
        }

        public struct Tile
        {
            public Tile(VectorInt2 position, CellDefinition<T> value)
            {
                Position = position;
                Cell = value;
            }

            public Tile((VectorInt2 p, CellDefinition<T> c) tuple) : this(tuple.p, tuple.c)
            {
            }

            public VectorInt2 Position { get;  }
            public CellDefinition<T> Cell { get;  }
        }

        public IEnumerable<Tile> ForEachTile()
        {
            foreach (var tp in this.ForEach())
            {
                yield return new Tile(tp);
            }
        }
        
        public CellDefinition<T>.Set Definition { get; }

        public Tile Player =>  new Tile(this.ForEach().First(x => x.Item2.IsPlayer));

        public RectInt Area => new RectInt(Map.Size);
        
        private CartesianMap<CellDefinition<T>> Map => this;        // I cannot decide between inheritance or composition
        
        public Bitmap ToMap(CellDefinition<T> c) 
            => Bitmap.Create(Map.Size, this.Map.ForEach().Where(x => x.Item2 == c).Select(x => x.Item1));
        
        public Bitmap ToMap(T c) 
            => Bitmap.Create(Map.Size, this.Map.ForEach().Where(x => x.Value.Equals(c)).Select(x => x.Item1));

        public Bitmap ToMap(IReadOnlyCollection<CellDefinition<T>> any) 
            => Bitmap.Create(Map.Size, this.Map.ForEach().Where(x => any.Contains(x.Item2)).Select(x => x.Item1));

        public Bitmap ToMap(params CellDefinition<T>[] any) 
            => Bitmap.Create(Map.Size, this.Map.ForEach().Where(x => any.Contains(x.Item2)).Select(x => x.Item1));

        public Bitmap ToMap(params T[] any) 
            => Bitmap.Create(Map.Size, this.Map.ForEach().Where(x => any.Contains(x.Item2.Underlying)).Select(x => x.Item1));
        

        public bool IsSolved => this.ForEach().Count(x=>x.Item2 == Definition.Crate) == 0;
        
        public bool IsValid(out string error)
        {
            if (ToMap(Definition.AllGoals).Count > ToMap(Definition.AllCrates).Count)
            {
                error = "More goals than crates";
                return false;
            }

            error = null;
            return true;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var y = 0; y < Map.Height; y++)
            {
                for (var x = 0; x < Map.Width; x++) sb.Append(Map[x, y]);
                sb.AppendLine();
            }

            return sb.ToString();
        }


        public List<string> ToStringList()
        {
            var res = new List<string>();
            for (var y = 0; y < Map.Height; y++)
            {
                var sb = new StringBuilder();
                for (var x = 0; x < Map.Width; x++) sb.Append(Map[x, y]);
                res.Add(sb.ToString());
            }

            return res;
        }
    }
}