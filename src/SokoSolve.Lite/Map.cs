using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SokoSolve.Lite
{
    public class Map
    {
        private readonly Cell[,] map;
        
        public Map(Cell[,] map)
        {
            this.map = map;
        }

        public Cell this[VectorInt2 p]
        {
            get => map[p.X, p.Y];
            set => map[p.X, p.Y] = value;
        }

        public bool Is(VectorInt2   p, Cell  cell) => (map[p.X, p.Y] & cell) == cell;
        public bool Is(VectorInt2   p, Block blk)  => ((int)map[p.X, p.Y] & (int)blk) > 0;

        public bool IsEmptyFloor(VectorInt2 p) => this[p] == Cell.Floor || this[p] == Cell.FloorGoal; 
        public bool IsCrate(VectorInt2      p) => this[p] == Cell.FloorCrate || this[p] == Cell.FloorGoalCrate;
        
        
        public void UnSetFlag(Block blk, VectorInt2 p)
        {
            var c = (int)this[p];
            
            var cc =c &  ~(int)blk;

            this[p] = (Cell)cc;
        }
        
        public void SetFlag(Block blk, VectorInt2 p)
        {
            var c = (int)this[p];
            
            var cc = c | (int)blk;

            this[p] = (Cell)cc;
        }

        public IEnumerable<Block> GetBlocks(VectorInt2 p) => throw new NotImplementedException();
        

        public VectorInt2 Player   => EachCell().First(x => x.c == Cell.FloorPlayer || x.c == Cell.FloorGoalPlayer).p;


        public bool IsSolved => EachPosition().Where(x => Is(x, Block.Goal)).All(x => Is(x, Block.Crate));

        public bool Contains(VectorInt2 pp) => pp.X >= 0 && pp.X <= map.GetLength(0)
                                                         && pp.Y >= 0 && pp.Y <= map.GetLength(1);
        

    

        public IEnumerable<(VectorInt2 p, Cell c)> EachCell()
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    yield return (new VectorInt2(x, y), map[x, y]);
                }   
            }
        }
        
        public IEnumerable<(VectorInt2 p, Block b)> EachBlock()
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    var cell = map[x, y];
                    foreach (var blk in Util.ToBlocks(cell))
                    {
                        yield return (new VectorInt2(x, y), blk);    
                    }
                }   
            }
        }

        public IEnumerable<VectorInt2> EachPosition()
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    yield return new VectorInt2(x, y);
                }   
            }
        }

        public IEnumerable<(VectorInt2 p, Cell c)> EachCell(Block b) =>
            EachPosition().Where(x => Is(x, b)).Select(x => (x, this[x]));
        
        
        public override string ToString() => ToString(Definition.Default);

        public string ToString(Definition def)
        {
            var sb = new StringBuilder();

            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (y > 0) sb.AppendLine();
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    sb.Append(def.ToChar(map[x, y]));
                }

                
            }

            return sb.ToString();
        }

        public Map  Clone() => new Map((Cell[,])map.Clone());
        
        // Move to Analystics
        //public bool Validate(out string[] errors) => throw new NotImplementedException();

    }
}