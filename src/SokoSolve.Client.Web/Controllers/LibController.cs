using System.Collections.Generic;
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

        // GET
        public IActionResult Index()
        {
            return View(compLib.GetDefaultLibraryCollection());
        }

        public class HomeModel
        {
            public Library Library { get; set; }
            public IReadOnlyDictionary<string, List<SolutionDTO>> Solutions { get; set; }
        }

        public IActionResult Home(string id)
        {
            var l = compLib.GetLibraryWithCaching(id);
            
            return View(new HomeModel()
            {
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