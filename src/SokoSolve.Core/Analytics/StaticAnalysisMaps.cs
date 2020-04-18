using System.Collections.Generic;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Analytics
{
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
            RecessMaps       = StaticAnalysis.FindRecesses(this);
            RecessMap = new Bitmap(puzzle.Size);
            foreach (var recessMap in RecessMaps)
            {
                RecessMap.SetBitwiseOR(RecessMap, recessMap);
            }
            DeadMap         = DeadMapAnalysis.FindDeadMap(this);
        }

        

        // Calculated
        public IBitmap          CornerMap       { get;  }
        public IBitmap          DoorMap         { get;  }
        public IBitmap          SideMap         { get;  }
        public List<LineBitmap> IndividualWalls { get;  }
        public List<LineBitmap> RecessMaps       { get;  }
        public IBitmap          DeadMap         { get;  }
        public IBitmap RecessMap { get; set; }
        public Map<float>?       Weightings      { get;  }


       
    }
}