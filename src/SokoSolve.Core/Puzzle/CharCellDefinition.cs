using System;
using System.Collections.Generic;

namespace SokoSolve.Core.Puzzle
{

    public abstract class CellDefinition<T>
    {
        public CellDefinition(T @void, T wall, T floor, T goal, T crateFloor, T crateGoalFloor, T playerFloor, T playerGoalFloor)
        {
            Void = @void;
            Wall = wall;
            Floor = floor;
            Goal = goal;
            CrateFloor = crateFloor;
            CrateGoalFloor = crateGoalFloor;
            PlayerFloor = playerFloor;
            PlayerGoalFloor = playerGoalFloor;

            AllFloors = new[] {Floor, Goal, CrateFloor, CrateGoalFloor, PlayerFloor, PlayerGoalFloor};
            
            AllGoals = new[] { Goal,  CrateGoalFloor,  PlayerGoalFloor};
            
            AllCrates = new[] { CrateFloor, CrateGoalFloor};
        }
        
        public 

        // Static
        public T Void { get; }
        public T Wall { get; }
        public T Floor { get; }
        public T Goal { get; }

        // Dynamic
        public T CrateFloor { get; }
        public T CrateGoalFloor { get; }
        public T PlayerFloor { get; }
        public T PlayerGoalFloor { get; }

        public bool IsFloor(T x) => 
            x.Equals(Floor) 
            || x.Equals(CrateFloor) 
            || x.Equals(CrateGoalFloor) 
            || x.Equals(PlayerFloor) 
            || x.Equals(PlayerGoalFloor);

        public bool IsCrate(T x) =>
            x.Equals(CrateFloor)
            || x.Equals(CrateGoalFloor); 

        public bool IsGoal(T x) => 
            x.Equals(Goal) 
            || x.Equals(CrateGoalFloor) 
            || x.Equals(PlayerGoalFloor);
        
        public bool IsPlayer(T x) => 
            x.Equals(PlayerFloor) 
            || x.Equals(PlayerGoalFloor);
        
        // States
        public IReadOnlyCollection<T> AllFloors { get; }
        public IReadOnlyCollection<T> AllGoals { get; }
        public IReadOnlyCollection<T> AllCrates { get; }
        
        
        public bool IsEmpty(char c)
        {
            return c == Floor || c == Goal;
        }

    }

    public class CharCellDefinition : CellDefinition<char>
    {
        public CharCellDefinition(char @void, char wall, char floor, char goal, char crateFloor, char crateGoalFloor, char playerFloor, char playerGoalFloor) : base(@void, wall, floor, goal, crateFloor, crateGoalFloor, playerFloor, playerGoalFloor)
        {
        }

        public CharCellDefinition() : this('~', '#', '.', 'O', 'X',  '$', 'P', '*')
        {
        }

        public static readonly CharCellDefinition Default = new CharCellDefinition();
        
        
        
        public char[] Seperate(char state)
        {
            // Singles
            if (state == Wall) return new[] {Wall};
            if (state == Void) return new[] {Void};
            if (state == Floor) return new[] {Floor};

            // Compound
            if (state == Goal) return new[] {Floor, Goal};
            if (state == Crate) return new[] {Floor, Crate};
            if (state == CrateGoal) return new[] {Floor, Goal, Crate};
            if (state == Player) return new[] {Floor, Player};
            if (state == PlayerGoal) return new[] {Floor, Player, Goal};

            throw new Exception(state.ToString());
        }
    }
}