using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core
{
    
    public enum MoveResult
    {
        Invalid,
        OkStep,
        OkPush,
        Win,
        Dead,
        
        Differed
    }
    
    public class SokobanGameLogic
    {
        public SokobanGameLogic(Puzzle start)
        {
            PuzzleStack = new Stack<Puzzle>();
            
            Statistics = new Statistics
            {
                Started   = DateTime.Now,
                Completed = DateTime.MinValue
            };
            
            Current = Start = start;
        }

        private   Stack<Puzzle>     PuzzleStack { get; }
        public    Statistics        Statistics  { get; protected set; }
        public    Puzzle            Current     { get; protected set; }
        public    Puzzle            Start       { get; protected set; }

        private void UpdateState(Puzzle newState)
        {
            PuzzleStack.Push(newState);
            Current = newState;
        }

        public virtual void Init(Puzzle puzzle)
        {
            if (puzzle == null) throw new ArgumentNullException("puzzle");

            Start = Current = puzzle;

            PuzzleStack.Clear();
            PuzzleStack.Push(puzzle);
        }

        public virtual MoveResult Move(VectorInt2 direction)
        {
            if (direction != VectorInt2.Up && direction != VectorInt2.Down &&
                direction != VectorInt2.Left && direction != VectorInt2.Right)
                throw new Exception("Must be U,D,L,R");

            var p   = Current.Player.Position;
            var pp  = p + direction;
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
                return MoveResult.OkStep;
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
                return MoveResult.OkPush;
            }

            return MoveResult.Invalid;
        }

        protected virtual void MoveCrate(Puzzle newState, VectorInt2 pp, VectorInt2 ppp)
        {
            Statistics.Pushes++;
            if (newState[pp] == newState.Definition.Crate) newState[pp] = newState.Definition.Floor;
            else if (newState[pp] == newState.Definition.CrateGoal) newState[pp] = newState.Definition.Goal;

            // Move to
            if (newState[ppp] == newState.Definition.Floor) newState[ppp] = newState.Definition.Crate;
            else if (newState[ppp] == newState.Definition.Goal) newState[ppp] = newState.Definition.CrateGoal;
        }

        protected virtual void MovePlayer(Puzzle newState, VectorInt2 p, VectorInt2 pp)
        {
            Statistics.Steps++;
            if (newState[p] == newState.Definition.Player)
                newState[p] = newState.Definition.Floor;
            else if (newState[p] == newState.Definition.PlayerGoal) newState[p] = newState.Definition.Goal;

            // Move to
            if (newState[pp] == newState.Definition.Floor)
                newState[pp] = newState.Definition.Player;
            else if (newState[pp] == newState.Definition.Goal) newState[pp] = newState.Definition.PlayerGoal;
        }

        public virtual bool UndoMove()
        {
            if (PuzzleStack.Count < 2) return false;

            Statistics.Undos++;
            
            PuzzleStack.Pop(); // Discard top
            Current = PuzzleStack.Peek();

            return true;
        }

        public virtual void Reset()
        {
            Statistics.Restarts++;
            
            Current = Start;
            PuzzleStack.Clear();
        }
    }
}