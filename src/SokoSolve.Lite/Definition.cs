using System.IO;

namespace SokoSolve.Lite
{
    public class Definition
    {
        public Definition()
        {
            Void       = '~';
            Wall       = '#';
            Floor      = '.';
            Crate      = 'X';
            Goal       = 'O';
            Player     = 'P';
            CrateGoal  = '$';
            PlayerGoal = '*';
        }

        public static Definition Default => new Definition();

        public Definition(string all)
        {
            if (string.IsNullOrWhiteSpace(all) || all.Length != 8) throw new InvalidDataException();
            Void       = all[0];
            Wall       = all[1];
            Floor      = all[2];
            Crate      = all[3];
            Goal       = all[4];
            Player     = all[5];
            CrateGoal  = all[6];
            PlayerGoal = all[7];
        }

        public char Void       { get; set; }
        public char Wall       { get; set; }
        public char Floor      { get; set; }
        public char Crate      { get; set; }
        public char Goal       { get; set; }
        public char Player     { get; set; }
        public char CrateGoal  { get; set; }
        public char PlayerGoal { get; set; }

        public Cell ToCell(char c)
        {
            if (c == Void) return Cell.Void;
            if (c == Wall) return Cell.Wall;
            if (c == Floor) return Cell.Floor;
            if (c == Goal) return Cell.FloorGoal;
            if (c == Crate) return Cell.FloorCrate;
            if (c == Player) return Cell.FloorPlayer;
            if (c == CrateGoal) return Cell.FloorGoalCrate;
            if (c == PlayerGoal) return Cell.FloorGoalPlayer;

            throw new InvalidDataException($"Invalid '{c}'");
        }

        public char ToChar(Cell cell)
        {
            if (cell == Cell.Void) return Void;
            if (cell == Cell.Wall) return Wall;
            if (cell == Cell.Floor) return Floor;
            if (cell == Cell.FloorGoal) return Goal;
            if (cell == Cell.FloorCrate) return Crate;
            if (cell == Cell.FloorPlayer) return Player;
            if (cell == Cell.FloorGoalCrate) return CrateGoal;
            if (cell == Cell.FloorGoalPlayer) return PlayerGoal;

            throw new InvalidDataException();
        }
    }
}
