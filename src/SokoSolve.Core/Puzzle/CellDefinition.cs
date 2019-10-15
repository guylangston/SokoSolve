using System;
using System.Collections;
using System.Collections.Generic;

namespace SokoSolve.Core.Puzzle
{
    public  class CellDefinition<T> : IEquatable<CellDefinition<T>>
    {
        public CellDefinition(T underlying, Set memberOf)
        {
            Underlying = underlying;
            MemberOf = memberOf;
        }

        public T Underlying { get; }
        public Set MemberOf { get;  }    // May or may not be a static (enum=static, theme=char)

        public class Set : IEnumerable<CellDefinition<T>>
        {
            public Set(T @void, T wall, T floor, T goal, T crate, T crateGoal, T player, T playerGoal)
            {
                Void = new CellDefinition<T>(@void, this);
                Wall = new CellDefinition<T>(wall, this);
                Floor = new CellDefinition<T>(floor, this);
                Goal = new CellDefinition<T>(goal, this);
                Crate = new CellDefinition<T>(crate, this);
                CrateGoal = new CellDefinition<T>(crateGoal, this);
                Player = new CellDefinition<T>(player, this);
                PlayerGoal =new CellDefinition<T>(playerGoal, this);

                AllFloors = new[] {Floor, Goal, Crate, Player, CrateGoal, PlayerGoal};
                AllCrates = new[] {Crate, CrateGoal};
                AllGoals = new[] {Goal, CrateGoal};
            }

            // Static
            public  CellDefinition<T> Void { get; }
            public  CellDefinition<T> Wall { get; }
            public  CellDefinition<T> Floor { get; }
            public  CellDefinition<T> Goal { get; }

            // Dynamic
            public  CellDefinition<T> Crate { get; }
            public  CellDefinition<T> CrateGoal { get; }
            public  CellDefinition<T> Player { get; }
            public  CellDefinition<T> PlayerGoal { get; }
        
            public  IReadOnlyCollection<CellDefinition<T>> AllFloors { get; }
            public  IReadOnlyCollection<CellDefinition<T>> AllGoals { get; }
            public  IReadOnlyCollection<CellDefinition<T>> AllCrates { get; }
            public IEnumerator<CellDefinition<T>> GetEnumerator()
            {
                yield return Void;
                yield return Wall;
                yield return Floor;
                yield return Goal;
                yield return Crate;
                yield return Player;
                yield return CrateGoal;
                yield return PlayerGoal;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public IReadOnlyCollection<T> Decompose()
        {
            throw new NotImplementedException();
        }

        public bool IsFloor =>
            Underlying.Equals(MemberOf.Floor)
            || Underlying.Equals(MemberOf.Crate)
            || Underlying.Equals(MemberOf.Goal)
            || Underlying.Equals(MemberOf.Player)
            || Underlying.Equals(MemberOf.CrateGoal)
            || Underlying.Equals(MemberOf.PlayerGoal);

        public bool IsCrate =>
            Underlying.Equals(MemberOf.Crate)
            || Underlying.Equals(MemberOf.CrateGoal); 

        public bool IsGoal => 
            Underlying.Equals(MemberOf.Goal) 
            || Underlying.Equals(MemberOf.CrateGoal) 
            || Underlying.Equals(MemberOf.PlayerGoal);
        
        public bool IsPlayer => 
            Underlying.Equals(MemberOf.Player) 
            || Underlying.Equals(MemberOf.PlayerGoal);
        
        public bool IsEmpty => 
            Underlying.Equals(MemberOf.Floor) 
            || Underlying.Equals(MemberOf.Goal);

        
        
        public bool Equals(CellDefinition<T> other)
        {
            if (other is null) return false;
            return MemberOf.Equals(other.MemberOf) && Underlying.Equals(other.Underlying);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CellDefinition<T>) obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}