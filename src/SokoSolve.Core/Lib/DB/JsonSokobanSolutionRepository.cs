using System.Collections.Generic;
using Newtonsoft.Json;

namespace SokoSolve.Core.Lib.DB
{



    public interface ISokobanSolutionRepository
    {
        IReadOnlyCollection<SolutionDTO>? GetPuzzleSolutions(PuzzleIdent puzzleId);
        IReadOnlyDictionary<string, List<SolutionDTO>> GetAll();

        void Store(SolutionDTO sol);
        
    }
    
    public class JsonSokobanSolutionRepository : ISokobanSolutionRepository
    {
        private string file;
        private JsonDataRoot data;


        class JsonDataRoot
        {
            public int NextId { get; set; } = 1;
            public Dictionary<string, List<SolutionDTO>> ByPuzzles { get; set; } = new Dictionary<string, List<SolutionDTO>>();
        }

        public JsonSokobanSolutionRepository(string file)
        {
            this.file = file;
            data      = JsonConvert.DeserializeObject<JsonDataRoot>(System.IO.File.ReadAllText(file));
        }

      
        public IReadOnlyCollection<SolutionDTO>? GetPuzzleSolutions(PuzzleIdent puzzleId)
        {
            if (data.ByPuzzles.TryGetValue(puzzleId.ToString(), out var s)) return s;
            return null;

        }

        public IReadOnlyDictionary<string, List<SolutionDTO>> GetAll()
        {
            return data.ByPuzzles;
        }

        public void Store(SolutionDTO sol)
        {
            if (sol.SolutionId == 0) sol.SolutionId = data.NextId++; 
            if (data.ByPuzzles.TryGetValue(sol.PuzzleIdent, out var s))
            {
                var exists = s.IndexOf(sol);
                if (exists < 0)
                {
                    s.Add(sol);    
                }
                else
                {
                    s[exists] = sol;
                }

            }
            else
            {
                data.ByPuzzles[sol.PuzzleIdent] = new List<SolutionDTO>()
                {
                    sol
                }; 
            }
            
            System.IO.File.WriteAllText(file, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

    }
}