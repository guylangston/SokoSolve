using System;
using System.Collections.Generic;

namespace SokoSolve.Base
{
    public enum MoveResult
    {
        Invalid,
        Ok,
        Win,
        Dead,
        InQueue
    }
     
     public class Statistics
     {
         public Statistics()
         {
             Started = Completed = DateTime.MinValue;
         }

         // Standard
         public int      Steps         { get; set; }
         public int      Pushes        { get; set; }
         public int      Undos         { get; set; }
         public int      Restarts      { get; set; }
         public DateTime Started       { get; set; }
         public DateTime Completed     { get; set; }
         public TimeSpan Elapased      => (Completed == DateTime.MinValue ? DateTime.Now : Completed) - Started;
         public double   DurationInSec => Elapased.TotalSeconds;


         public override string ToString() => $"Steps: {Steps}, Pushes: {Pushes}, Undos: {Undos}, Restarts: {Restarts}";
     }
    
    public class SokobanGameLogic
    {
        public SokobanGameLogic(Map start)
        {
            PuzzleStack = new Stack<Map>();
            
            Statistics = new Statistics
            {
                Started   = DateTime.Now,
                Completed = DateTime.MinValue
            };
            
            Current = Start = start;
        }

        private Stack<Map> PuzzleStack { get; }
        public  Statistics Statistics  { get; protected set; }
        public  Map        Current     { get; protected set; }
        public  Map        Start       { get; protected set; }

        private void UpdateState(Map newState)
        {
            PuzzleStack.Push(newState);
            Current = newState;
        }

        public virtual void Init()
        {
            PuzzleStack.Clear();
            PuzzleStack.Push(Start);
        }

        public virtual MoveResult Move(VectorInt2 direction)
        {
            if (direction != VectorInt2.Up && direction != VectorInt2.Down &&
                direction != VectorInt2.Left && direction != VectorInt2.Right)
                throw new Exception("Must be U,D,L,R");

            var p   = Current.Player;
            var pp  = p + direction;
            var ppp = pp + direction;

            // Valid?
            if (!Current.Contains(pp)) return MoveResult.Invalid;
            if (!Current.Contains(ppp)) return MoveResult.Invalid;

            // Move/Step
            if (Current.IsEmptyFloor(pp))
            {
                var newState = Current.Clone();
                MovePlayer(newState, p, pp);
                UpdateState(newState);
                return MoveResult.Ok;
            }

            // Push
            if (Current.IsCrate(pp))
            {
                if (!Current.IsEmptyFloor(ppp)) return MoveResult.Invalid;

                var newState = Current.Clone();
                MoveCrate(newState, pp, ppp);
                MovePlayer(newState, p, pp);
                UpdateState(newState);

                if (newState.IsSolved) return MoveResult.Win;
                return MoveResult.Ok;
            }

            return MoveResult.Invalid;
        }

        protected virtual void MoveCrate(Map newState, VectorInt2 pp, VectorInt2 ppp)
        {
            Statistics.Pushes++;
            
            newState.UnSetFlag(Block.Crate, pp);
            newState.SetFlag(Block.Crate, ppp);
        }

        protected virtual void MovePlayer(Map newState, VectorInt2 p, VectorInt2 pp)
        {
            Statistics.Steps++;
            
            newState.UnSetFlag(Block.Player, p);
            newState.SetFlag(Block.Player, pp);
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

    public static class Util
    {
        public static IEnumerable<VectorInt2> ToPath(string p)
        {
            foreach (var chr in p)
            {
                yield return chr switch
                {
                    'L' => VectorInt2.Left,
                    'R' => VectorInt2.Right,
                    'U' => VectorInt2.Up,
                    'D' => VectorInt2.Down,
                    _ => throw new InvalidCastException(chr.ToString())
                };
            }
        }
    }
}