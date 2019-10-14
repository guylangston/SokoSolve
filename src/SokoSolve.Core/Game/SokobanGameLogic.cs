using System;
using System.Collections.Generic;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Game
{
    public class SokobanGameLogic
    {
        public SokobanGameLogic()
        {
            PuzzleStack = new Stack<Puzzle.Puzzle>();
            MoveStack = new Stack<VectorInt2>();
            Statistics = new Statistics();
        }

        public SokobanGameLogic(Puzzle.Puzzle start) : this()
        {
            Current = Start = start;
        }

        public Statistics Statistics { get; protected set; }

        public Puzzle.Puzzle Current { get; protected set; }

        public Puzzle.Puzzle Start { get; protected set; }

        protected Stack<Puzzle.Puzzle> PuzzleStack { get; set; }
        protected Stack<VectorInt2> MoveStack { get; set; }


        private void UpdateState(Puzzle.Puzzle newState)
        {
            PuzzleStack.Push(Current);
            Current = newState;
        }


        public virtual MoveResult Move(VectorInt2 direction)
        {
            if (direction != VectorInt2.Up && direction != VectorInt2.Down
                                           && direction != VectorInt2.Left && direction != VectorInt2.Right)
                throw new Exception("Must be U,D,L,R");

            var p = Current.Player.Position;
            var pp = p + direction;
            var ppp = pp + direction;

            // Valid?
            if (!Current.Area.Contains(pp)) return MoveResult.Invalid;
            if (!Current.Area.Contains(ppp)) return MoveResult.Invalid;

            // Move/Step
            if (Current.Definition.IsEmpty(Current[pp]))
            {
                var newState = new Puzzle.Puzzle(Current);

                // Move away
                MovePlayer(newState, p, pp);

                UpdateState(newState);
                return MoveResult.Ok;
            }

            // Push
            if (Current.Definition.IsCrate(Current[pp]))
            {
                if (!Current.Definition.IsEmpty(Current[ppp])) return MoveResult.Invalid;

                var newState = new Puzzle.Puzzle(Current);

                MoveCrate(newState, pp, ppp);
                MovePlayer(newState, p, pp);

                UpdateState(newState);

                if (newState.IsSolved) return MoveResult.Win;
                return MoveResult.Ok;
            }

            return MoveResult.Invalid;
        }

        protected virtual void MoveCrate(Puzzle.Puzzle newState, VectorInt2 pp, VectorInt2 ppp)
        {
            Statistics.Pushes++;
            if (newState[pp] == newState.Definition.Crate)
                newState[pp] = newState.Definition.Floor;
            else if (newState[pp] == newState.Definition.CrateGoal) newState[pp] = newState.Definition.Goal;

            // Move to
            if (newState[ppp] == newState.Definition.Floor)
                newState[ppp] = newState.Definition.Crate;
            else if (newState[ppp] == newState.Definition.Goal) newState[ppp] = newState.Definition.CrateGoal;
        }


        protected virtual void MovePlayer(Puzzle.Puzzle newState, VectorInt2 p, VectorInt2 pp)
        {
            Statistics.Steps++;
            MoveStack.Push(pp);
            if (newState[p] == newState.Definition.Player)
                newState[p] = newState.Definition.Floor;
            else if (newState[p] == newState.Definition.PlayerGoal) newState[p] = newState.Definition.Goal;

            // Move to
            if (newState[pp] == newState.Definition.Floor)
                newState[pp] = newState.Definition.Player;
            else if (newState[pp] == newState.Definition.Goal) newState[pp] = newState.Definition.PlayerGoal;
        }
    }
}