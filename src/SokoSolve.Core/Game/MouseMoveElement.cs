using System.Linq;
using ConsoleZ.Win32;
using SokoSolve.Core.Analytics;
using VectorInt;
using VectorInt.Collections;

namespace SokoSolve.Core.Game
{
    public class MouseMoveElement : GameElement
    {
        public enum Action
        {
            None,
            Push,
            Move,
            Drag
        }

        protected bool       isDragInProgress;
        protected Path       peekCratePath;
        protected Path       peekMovePath;
        protected VectorInt2 prev = VectorInt2.MinValue;
        protected bool       prevLeftDown;
        protected bool       prevRightDown;
        protected VectorInt2 start = VectorInt2.MinValue;

        public MouseMoveElement(InputProvider input)
        {
            this.InputProvider = input;
            ZIndex             = 100;
        }

        public InputProvider InputProvider { get; set; }
        public Path WalkPath { get; set; }

        public void Drag(VectorInt2 cell)
        {
            start            = cell;
            isDragInProgress = true;
        }

        public override void Step(float elapsedSec)
        {
            // Is mouse over the puzzle?
            var mousePos = InputProvider.MousePosition;
            var cellPos  = mousePos - Game.PuzzleSurface.TL;
            if (!Game.HasMoves && Game.PuzzleSurface.Contains(mousePos) )
            {
                // Next to crate, can we push it?
                var cell = Game.Current[cellPos];
                if (cell.IsCrate && InputProvider.IsMouseClick)
                {
                    var player = VectorInt2.Directions.FirstOrDefault(x => Game.Current[cellPos + x].IsPlayer);
                    if (!player.IsZero)
                    {
                        Game.Move(player*new VectorInt2(-1));
                    }
                }
                else 
                {
                    // Mouse Move to a free floor position (No Pushes)
                    this.WalkPath = FindMoveMapOrNull(Game.Current, Game.Current.Player.Position, cellPos);
                    if (WalkPath != null && InputProvider.IsMouseClick && !Game.HasMoves)
                    {
                        foreach (var dir in WalkPath)
                        {
                            Game.Move(dir);
                        }

                        WalkPath = null; // Don't draw later
                    }
                }
            }

            base.Step(elapsedSec);
        }

        static Path FindMoveMapOrNull(Puzzle p, VectorInt2 start, VectorInt2 end)
        {
            var boundary = p.ToMap(p.Definition.Obsticles);
            return PathFinder.Find(boundary, start, end);
        }

        public override void Init()
        {
            base.Init();
        }

        public void UpdateMouseWithLogicalCell(VectorInt2 cell, bool isLeftDown, bool isRightDown)
        {
            if (!Game.Current.Contains(cell)) return;

            peekMovePath  = null;
            peekCratePath = null;
            try
            {
                if (isLeftDown && !prevLeftDown)
                {
                    Drag(cell);
                    return;
                }

                if (!isLeftDown && prevLeftDown)
                {
                    Drop(cell);
                    return;
                }

                var peek = Peek(cell);
                if (peek != Action.None)
                {
                    if (peek == Action.Move)
                    {
                        start = Game.Current.Player.Position;
                        var end = cell;
                        var boundry = Game.Current.ToMap(Game.Current.Definition.Wall, Game.Current.Definition.Crate,
                            Game.Current.Definition.CrateGoal);
                        peekMovePath = PathFinder.Find(boundry, start, end);
                    }
                    else if (peek == Action.Drag)
                    {
                        var end     = cell;
                        var state   = Game.Analysis.Evalute(Game.Current);
                        var pushMap = PushMap.Find(state.Static, state.Current, start, Game.Current.Player.Position);
                        if (pushMap.CrateMap[end])
                        {
                            //var walk = pushMap.FindPlayerWalkRoute(end);
                            peekCratePath = pushMap.FindCrateRoute(end);

                            // PLayer move to begin crate stuff

                            var pstart = Game.Current.Player.Position;
                            var pend   = start - peekCratePath.First();
                            var boundry = Game.Current.ToMap(Game.Current.Definition.Wall,
                                Game.Current.Definition.Crate,
                                Game.Current.Definition.CrateGoal);
                            peekMovePath = PathFinder.Find(boundry, pstart, pend);
                            if (peekMovePath != null) peekMovePath.Add(peekCratePath.First());
                        }
                    }
                }
            }
            finally
            {
                prev          = cell;
                prevLeftDown  = isLeftDown;
                prevRightDown = isRightDown;
            }
        }

        public Action Peek(VectorInt2 currentMouseCell)
        {
            var isDrag      = start != VectorInt2.MinValue && start != currentMouseCell;
            var singleClick = start == currentMouseCell;

            if (Game.Current.Contains(start))
            {
                if (singleClick)
                {
                    // Simple Push: Am I next to a crate
                    if (Game.Current[currentMouseCell].IsCrate)
                    {
                        var dir = currentMouseCell - Game.Current.Player.Position;
                        if (dir.IsUnit) // Next 
                            return Action.Push;
                    }

                    // Just click on a floor cell
                    if (Game.Current[currentMouseCell] == Game.Current.Definition.Floor ||
                        Game.Current[currentMouseCell] == Game.Current.Definition.Goal)
                        return Action.Move;
                }

                if (isDrag)
                    // Crate drag: start dragging a crate, to a cell that is empty
                    if (Game.Current[start].IsCrate)
                        if (Game.Current[currentMouseCell].IsEmpty ||
                            currentMouseCell == Game.Current.Player.Position)
                            return Action.Drag;

                // No clicks: Assume move
                if (currentMouseCell != Game.Current.Player.Position &&
                    (Game.Current[currentMouseCell] == Game.Current.Definition.Floor ||
                     Game.Current[currentMouseCell] == Game.Current.Definition.Goal))
                    return Action.Move;
            }

            return Action.None;
        }

        public void Drop(VectorInt2 cell)
        {
            var action = Peek(cell);
            try
            {
                if (action == Action.Push)
                {
                    var dir = cell - Game.Current.Player.Position;
                    if (dir.IsUnit) // Next to each other
                        Game.Move(dir);
                }
                else if (action == Action.Move)
                {
                    start = Game.Current.Player.Position;
                    var end = cell;
                    var boundry = Game.Current.ToMap(Game.Current.Definition.Wall, Game.Current.Definition.Crate,
                        Game.Current.Definition.CrateGoal);
                    var path = PathFinder.Find(boundry, start, end);
                    if (path != null)
                        foreach (var step in path)
                            Game.Move(step);
                }
                else if (action == Action.Drag)
                {
                    var end   = cell;
                    var state = Game.Analysis.Evalute(Game.Current);

                    var pushMap = PushMap.Find(state.Static, state.Current, start, Game.Current.Player.Position);
                    if (pushMap.CrateMap[end])
                    {
                        // Do Moves
                        var path = pushMap.FindPlayerWalkRoute(end);
                        if (path != null)
                            foreach (var c in path)
                                Game.Move(c);
                    }
                }
            }
            finally
            {
                // Finally
                isDragInProgress = false;
                start            = VectorInt2.MinValue;
            }
        }
    }
}