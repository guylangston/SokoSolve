namespace SokoSolve.Core.Game
{
    
    public enum PuzzlePiece
    {
        // Static
        Void     = 0b0000_0001,
        Wall     = 0b0000_0010,
        Floor    = 0b0000_0100,
        Goal     = 0b0000_1000,
        
        // Dynamic
        Crate     = 0b0001_0000,
        Player    = 0b0010_0000,
    }

    
    public enum CellEnum
    {
        // Static
        Void             = 0b0000_0001,
        Wall             = 0b0000_0010,
        Floor            = 0b0000_0100,
        Goal             = 0b0000_1000,
        
        // Dynamic
        CrateFloor       = 0b0001_0100,
        CrateGoalFloor   = 0b0001_1100,
        PlayerFloor      = 0b0010_0100,
        PlayerGoalFloor  = 0b0010_1100,
    }
    
    public class EnumCellDefinition : CellDefinition<CellEnum>
    {
        private EnumCellDefinition(CellEnum underlying, Set memberOf) : base(underlying, memberOf)
        {
        }

        public static readonly Set Owner = new Set(
            CellEnum.Void,
            CellEnum.Wall,
            CellEnum.Floor,
            CellEnum.Goal,
            CellEnum.CrateFloor,
            CellEnum.CrateGoalFloor,
            CellEnum.PlayerFloor,
            CellEnum.PlayerGoalFloor
        ); 
        
        
        
    }
}