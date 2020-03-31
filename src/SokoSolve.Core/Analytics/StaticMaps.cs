using System.Collections.Generic;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Analytics
{
    public class StaticMaps
    {
        public  StaticMaps(IBitmap wallMap, IBitmap floorMap, IBitmap goalMap, IBitmap crateStart)
        {
            WallMap    = wallMap;
            FloorMap   = floorMap;
            GoalMap    = goalMap;
            CrateStart = crateStart;
        }

       
        public IBitmap WallMap    { get;  }
        public IBitmap FloorMap   { get;  }
        public IBitmap GoalMap    { get;  }
        public IBitmap CrateStart { get; }

    }
    
    public class StaticAnalysisMaps : StaticMaps
    {
        public StaticAnalysisMaps(Puzzle puzzle) 
            : base(
                puzzle.ToMap(puzzle.Definition.Wall, puzzle.Definition.Void),
                puzzle.ToMap(puzzle.Definition.AllFloors),
                puzzle.ToMap(puzzle.Definition.AllGoals),
                puzzle.ToMap(puzzle.Definition.AllCrates))
        {
            CornerMap       = StaticAnalysis.FindCorners(this);
            DoorMap         = StaticAnalysis.FindDoors(this);
            SideMap         = StaticAnalysis.FindSides(this);
            IndividualWalls = StaticAnalysis.FindWalls(this);
            RecessMap       = StaticAnalysis.FindRecesses(this);
            DeadMap         = DeadMapAnalysis.FindDeadMap(this);
        }

        // Calculated
        public IBitmap          CornerMap       { get;  }
        public IBitmap          DoorMap         { get;  }
        public IBitmap          SideMap         { get;  }
        public List<LineBitmap> IndividualWalls { get;  }
        public List<LineBitmap> RecessMap       { get;  }
        public IBitmap          DeadMap         { get;  }
        public Map<float>?       Weightings      { get;  }


       
    }
}