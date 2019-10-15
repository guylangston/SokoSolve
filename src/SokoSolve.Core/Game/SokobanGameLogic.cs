using System;
using System.Collections.Generic;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Game
{
    public class SokobanGameLogic
    {
        public SokobanGameLogic()
        {
            PuzzleStack = new Stack<Puzzle>();
            MoveStack = new Stack<VectorInt2>();
            Statistics = new Statistics();
        }

        public SokobanGameLogic(Puzzle start) : this()
        {
            Current = Start = start;
        }

        public Statistics Statistics { get; protected set; }

        public Puzzle Current { get; protected set; }

        public Puzzle Start { get; protected set; }

        protected Stack<Puzzle> PuzzleStack { get; set; }
        protected Stack<VectorInt2> MoveStack { get; set; }


        private void UpdateState(Puzzle newState)
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
            if (Current[pp].IsEmpty)
            {
                var newState = Current.Clone();

                // Move away
                MovePlayer(newState, p, pp);

                UpdateState(newState);
                return MoveResult.Ok;
            }

            // Push
            if (Current[pp].IsCrate)
            {
                if (!Current[ppp].IsEmpty) return MoveResult.Invalid;

                var newState = Current.Clone();

                MoveCrate(newState, pp, ppp);
                MovePlayer(newState, p, pp);

                UpdateState(newState);

                if (newState.IsSolved) return MoveResult.Win;
                return MoveResult.Ok;
            }

            return MoveResult.Invalid;
        }

        protected virtual void MoveCrate(Puzzle newState, VectorInt2 pp, VectorInt2 ppp)
        {
            Statistics.Pushes++;
            if (newState[pp] == newState.Definition.Crate)
                newState[pp] = (CharCellDefinition)newState.Definition.Floor;
            else if (newState[pp] == newState.Definition.CrateGoal) newState[pp] = (CharCellDefinition)newState.Definition.Goal;

            // Move to
            if (newState[ppp] == newState.Definition.Floor)
                newState[ppp] = (CharCellDefinition)newState.Definition.Crate;
            else if (newState[ppp] == newState.Definition.Goal) newState[ppp] = (CharCellDefinition)newState.Definition.CrateGoal;
        }


        protected virtual void MovePlayer(Puzzle newState, VectorInt2 p, VectorInt2 pp)
        {
            Statistics.Steps++;
            MoveStack.Push(pp);
            if (newState[p] == newState.Definition.Player)
                newState[p] = (CharCellDefinition)newState.Definition.Floor;
            else if (newState[p] == newState.Definition.PlayerGoal) newState[p] = (CharCellDefinition)newState.Definition.Goal;

            // Move to
            if (newState[pp] == newState.Definition.Floor)
                newState[pp] = (CharCellDefinition)newState.Definition.Player;
            else if (newState[pp] == newState.Definition.Goal) newState[pp] = (CharCellDefinition)newState.Definition.PlayerGoal;
        }
    }
}