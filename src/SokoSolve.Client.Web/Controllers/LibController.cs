using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;

namespace SokoSolve.Client.Web.Controllers
{
    public class LibController : Controller
    {
        private readonly LibraryComponent compLib;
        private readonly ISokobanSolutionRepository repSol;

        public LibController(LibraryComponent compLib, ISokobanSolutionRepository repSol)
        {
            this.compLib = compLib;
            this.repSol = repSol;
        }

        public class IndexModel
        {
            public LibraryCollection Collection { get; set; }
            public IReadOnlyDictionary<string, List<SolutionDTO>> Solutions { get; set; }

            public SolutionDTO GetLatest(string lib)
            {
                return Solutions.Where(x => x.Key.StartsWith(lib + "~"))
                                .SelectMany(x => x.Value)
                                .Where(x => x.HasSolution)
                                .OrderByDescending(x => x.Modified).FirstOrDefault();
            }
            
            public IEnumerable<(string puzzle, SolutionDTO sol)> Get(string lib)
            {
                foreach (var s in Solutions)
                {
                    if (s.Key.StartsWith(lib + "~"))
                    {
                        var sol = s.Value.LastOrDefault(x => x.HasSolution);
                        if (sol != null)
                        {
                            yield return (s.Key, sol);
                        }
                        else
                        {
                            yield return (s.Key, null);
                        }
                    }
                }
            }
        }

        
        public IActionResult Index()
        {
            return View(new IndexModel()
            {
                Collection = compLib.GetDefaultLibraryCollection(),
                Solutions = repSol.GetAll()
            });
        }

        public class HomeModel
        {
            public string                                         Id         { get; set; }
            public Library                                        Library   { get; set; }
            public IReadOnlyDictionary<string, List<SolutionDTO>> Solutions { get; set; }
        }

        public IActionResult Home(string id)
        {
            var l = compLib.GetLibraryWithCaching(id);
            
            return View(new HomeModel()
            {
                Id = id,
                Library =  l,
                Solutions = repSol.GetAll()
            });
        }
        
        
        public IActionResult Solutions(string id)
        {
            var l = compLib.GetLibraryWithCaching(id);
            
            return View(new HomeModel()
            {
                Library   =  l,
                Solutions = repSol.GetAll()
            });
        }

       
    }
}