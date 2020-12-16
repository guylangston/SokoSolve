using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using SokoSolve.Drawing;
using VectorInt;

namespace SokoSolve.Client.Web.Logic
{

    public class ServerSideGame : SokobanGameLogic
    {
        private RectInt surface;

        public ServerSideGame(LibraryPuzzle start) : base(start.Puzzle)
        {
            LibraryPuzzle = start;
            this.surface  = new RectInt(CellSize * start.Puzzle.Size);
        }
        
        public LibraryPuzzle LibraryPuzzle { get; }

        public VectorInt2 CellSize { get; set; } = new VectorInt2(32);

        public MoveResult Click(VectorInt2 at)
        {
            if (!surface.Contains(at)) return MoveResult.Invalid;

            var pp = at / CellSize;
            var rel = pp - Current.Player.Position;
            if (Math.Abs(rel.X) + Math.Abs(rel.Y) == 1) // Is Adjacent?
            {
                if (Current[pp].IsFloor)
                {
                    return Move(rel);
                }
            }
            
            if (Current[pp].IsFloor)
            {
                // Can we move here?
                if (CanMoveWithoutPushing(Current, pp, out var steps))
                {
                    var last = MoveResult.Invalid;
                    foreach (var s in steps)
                    {
                        last = Move(s);
                        if (last != MoveResult.OkStep) throw new Exception("Bad Path");

                    }
                    return last;
                }
            }

            return MoveResult.Invalid;
        }
        
        private bool CanMoveWithoutPushing(Puzzle current, VectorInt2 pp, out IEnumerable<VectorInt2> steps)
        {
            var map = SolverHelper.FloodFillUsingWallAndCrates(
                current.ToMap(current.Definition.Wall),
                current.ToMap(current.Definition.Crate),
                current.Player.Position);
            if (map[pp])
            {
                var path = SolverHelper.FindPath(map, current.Player.Position, pp);
                if (path != null)
                {
                    steps = path;
                    return true;
                }

            }
            steps = default;
            return false;
        }


        public string Draw()
        {
            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb))
            {
                var dia = new PuzzleDiagram()
                {
                    SkipHeader  = true,
                    GetResource = x => "/img/"+x
                };
                dia.Draw(tw, Current, CellSize);    
            }
            return sb.ToString();
        }
    }
}