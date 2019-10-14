using System;

namespace SokoSolve.Core.Puzzle
{
    public class CellDefinition
    {
        public static readonly CellDefinition Default = new CellDefinition();
        public readonly char Crate = 'X';
        public readonly char CrateGoal = '$';
        public readonly char Floor = '.';
        public readonly char Goal = 'O';
        public readonly char Player = 'P';
        public readonly char PlayerGoal = '*';

        public readonly char Void = '~';

        // Atomic
        public readonly char Wall = '#';

        // States
        public char[] AllFloors
        {
            get { return new[] {Floor, Crate, CrateGoal, Player, PlayerGoal, Goal}; }
        }

        public char[] AllGoals
        {
            get { return new[] {CrateGoal, PlayerGoal, Goal}; }
        }

        public char[] AllCrates
        {
            get { return new[] {CrateGoal, Crate}; }
        }


        // Helpers
        public bool IsFloor(char c)
        {
            return c == Floor || c == Crate || c == CrateGoal || c == Player || c == PlayerGoal || c == Goal;
        }

        public bool IsGoal(char c)
        {
            return c == CrateGoal || c == PlayerGoal || c == Goal;
        }

        public bool IsPlayer(char c)
        {
            return c == Player || c == PlayerGoal;
        }

        public bool IsCrate(char c)
        {
            return c == CrateGoal || c == Crate;
        }

        public bool IsEmpty(char c)
        {
            return c == Floor || c == Goal;
        }

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