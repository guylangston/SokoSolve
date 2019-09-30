using System;
using System.Linq;
using System.Runtime.InteropServices;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Game
{
    public class MouseController : GameElement
    {

        protected bool isDragInProgress = false;
        protected VectorInt2 start = VectorInt2.MinValue;
        protected VectorInt2 prev = VectorInt2.MinValue;
        protected bool prevLeftDown = false;
        protected bool prevRightDown = false;
        protected Path peekMovePath;
        protected Path peekCratePath;

        public enum Action
        {
            None,
            Push,
            Move,
            Drag
        }

        public void Drag(VectorInt2 cell)
        {
            start = cell;
            isDragInProgress = true;
        }

        public void UpdateMouseWithLogicalCell(VectorInt2 cell, bool isLeftDown, bool isRightDown)
        {
            if (!Game.Current.Contains(cell)) return;

            peekMovePath = null;
            peekCratePath = null;
            try
            {
                if (Game.HasPendingMoves)
                {
                    // No hints while there are outstanding actions
                    return;
                }

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
                        var end = cell;
                        var state = Game.Analysis.Evalute(Game.Current);
                        var pushMap = PushMap.Find(state.Static, state.Current, start, Game.Current.Player.Position);
                        if (pushMap.CrateMap[end])
                        {
                            //var walk = pushMap.FindPlayerWalkRoute(end);
                            peekCratePath = pushMap.FindCrateRoute(end);

                            // PLayer move to begin crate stuff
                            
                            var pstart = Game.Current.Player.Position;
                            var pend = start - peekCratePath.First();
                            var boundry = Game.Current.ToMap(Game.Current.Definition.Wall, Game.Current.Definition.Crate,
                                Game.Current.Definition.CrateGoal);
                            peekMovePath = PathFinder.Find(boundry, pstart, pend);
                            if (peekMovePath != null)
                            {
                                peekMovePath.Add(peekCratePath.First());
                            }
                        }
                    }
                }
            }
            finally
            {
                prev = cell;
                prevLeftDown = isLeftDown;
                prevRightDown = isRightDown;
            }
        }

        public Action Peek(VectorInt2 currentMouseCell)
        {
            bool isDrag =  start != VectorInt2.MinValue && start != currentMouseCell;
            bool singleClick = start == currentMouseCell;

            if (Game.Current.Contains(start))
            {



                if (singleClick)
                {
                    // Simple Push: Am I next to a crate
                    if (Game.Current.Definition.IsCrate(Game.Current[currentMouseCell]))
                    {
                        var dir = currentMouseCell - Game.Current.Player.Position;
                        if (dir.IsUnit) // Next 
                        {
                            return Action.Push;
                        }
                    }
                    // Just click on a floor cell
                    if (Game.Current[currentMouseCell] == Game.Current.Definition.Floor ||
                        Game.Current[currentMouseCell] == Game.Current.Definition.Goal)
                    {
                        return Action.Move;
                    }
                }

                if (isDrag)
                {
                    // Crate drag: start dragging a crate, to a cell that is empty
                    if (Game.Current.Definition.IsCrate(Game.Current[start]))
                    {
                        if (Game.Current.Definition.IsEmpty(Game.Current[currentMouseCell]) ||
                            currentMouseCell == Game.Current.Player.Position)
                        {
                            return Action.Drag;
                        }
                    }
                }

                // No clicks: Assume move
                if (currentMouseCell != Game.Current.Player.Position &&
                    (Game.Current[currentMouseCell] == Game.Current.Definition.Floor ||
                     Game.Current[currentMouseCell] == Game.Current.Definition.Goal))
                {
                    return Action.Move;
                }
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
                    {
                        Game.Move(dir);
                    }
                }
                else if (action == Action.Move)
                {
                    start = Game.Current.Player.Position;
                    var end = cell;
                    var boundry = Game.Current.ToMap(Game.Current.Definition.Wall, Game.Current.Definition.Crate,
                        Game.Current.Definition.CrateGoal);
                    var path = PathFinder.Find(boundry, start, end);
                    if (path != null)
                    {
                        foreach (var step in path)
                        {
                            Game.Move(step);
                        }
                    }
                }
                else if (action == Action.Drag)
                {
                    var end = cell;
                    var state = Game.Analysis.Evalute(Game.Current);

                    var pushMap = PushMap.Find(state.Static, state.Current, start, Game.Current.Player.Position);
                    if (pushMap.CrateMap[end])
                    {
                        // Do Moves
                        var path = pushMap.FindPlayerWalkRoute(end);
                        if (path != null)
                        {
                            foreach (var c in path)
                            {
                                Game.Move(c);
                            }
                        }
                    }
                }
            }
            finally
            {
                // Finally
                isDragInProgress = false;
                start = VectorInt2.MinValue;
            }
        }
    }
}