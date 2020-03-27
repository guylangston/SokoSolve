using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SokoSolve.Core
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

        public override string ToString() => Underlying.ToString();
        

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
                Obsticles = new[] {Void, Wall, Crate, CrateGoal, };
                All = new[] { Void, Wall, Floor, Goal, Crate, Player, CrateGoal, PlayerGoal };
            }

            // Static
            public CellDefinition<T> Void  { get; }
            public CellDefinition<T> Wall  { get; }
            public CellDefinition<T> Floor { get; }
            public CellDefinition<T> Goal  { get; }

            // Dynamic
            public CellDefinition<T> Crate      { get; }
            public CellDefinition<T> CrateGoal  { get; }
            public CellDefinition<T> Player     { get; }
            public CellDefinition<T> PlayerGoal { get; }

            public IReadOnlyList<CellDefinition<T>>       All             { get; }
            public IReadOnlyCollection<CellDefinition<T>> AllFloors       { get; }
            public IReadOnlyCollection<CellDefinition<T>> AllGoals        { get; }
            public IReadOnlyCollection<CellDefinition<T>> AllCrates       { get; }
            public IReadOnlyCollection<CellDefinition<T>> Obsticles       { get; }
            
            public IEnumerator<CellDefinition<T>> GetEnumerator() => All.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            
            public bool TryFromUnderlying(T c, out CellDefinition<T> cell)
            {
                foreach (var cc in this)
                {
                    if (cc.Underlying.Equals(c))
                    {
                        cell = cc;
                        return true;
                    };
                }

                cell = null;
                return false;
            }

            public CellDefinition<T> Get(T c)
            {
                return TryFromUnderlying(c, out var match) ? match : throw new InvalidDataException(c.ToString());
            }
        }

        public IReadOnlyCollection<CellDefinition<T>> Decompose()
        {
            if (this == MemberOf.Void)   return new[] {this};
            if (this == MemberOf.Wall)   return new[] {this};
            if (this == MemberOf.Floor)  return new[] {this};

            if (this == MemberOf.Crate)  return new[] {MemberOf.Floor, MemberOf.Crate};
            if (this == MemberOf.Goal)   return new[] {MemberOf.Floor, MemberOf.Goal};
            if (this == MemberOf.Player) return new[] {MemberOf.Floor, MemberOf.Player};
            
            if (this == MemberOf.CrateGoal) return new[] {MemberOf.Floor, MemberOf.Crate, MemberOf.Goal};
            if (this == MemberOf.PlayerGoal) return new[] {MemberOf.Floor, MemberOf.Player, MemberOf.Goal};
            
            throw new InvalidDataException();            
        }

        public bool IsFloor =>
            this.Equals(MemberOf.Floor)
            || this.Equals(MemberOf.Crate)
            || this.Equals(MemberOf.Goal)
            || this.Equals(MemberOf.Player)
            || this.Equals(MemberOf.CrateGoal)
            || this.Equals(MemberOf.PlayerGoal);

        public bool IsCrate =>
            this.Equals(MemberOf.Crate)
            || this.Equals(MemberOf.CrateGoal); 

        public bool IsGoal => 
            this.Equals(MemberOf.Goal) 
            || this.Equals(MemberOf.CrateGoal) 
            || this.Equals(MemberOf.PlayerGoal);
        
        public bool IsPlayer => 
            this.Equals(MemberOf.Player) 
            || this.Equals(MemberOf.PlayerGoal);
        
        public bool IsEmpty => 
            this.Equals(MemberOf.Floor) 
            || this.Equals(MemberOf.Goal);


        public static bool operator ==(CellDefinition<T> lhs, CellDefinition<T> rhs)
        {
            if (lhs is null && rhs is null) return true;
            if (lhs is null || rhs is null) return false;
            return lhs.Underlying.Equals(rhs.Underlying);
        }

        public static bool operator !=(CellDefinition<T> lhs, CellDefinition<T> rhs)
        {
            if (lhs is null && rhs is null) return false;
            if (lhs is null || rhs is null) return true;
            return !lhs.Underlying.Equals(rhs.Underlying);
        }

        public bool Equals(CellDefinition<T> other)
        {
            if (other is null) return false;
            return Underlying.Equals(other.Underlying);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is T tt) return Underlying.Equals(tt);
            return Equals((CellDefinition<T>) obj);
        }

        public override int GetHashCode() => Underlying.GetHashCode();
    }
}