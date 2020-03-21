using System;
using Microsoft.AspNetCore.Mvc;
using SokoSolve.Core.Lib;

namespace SokoSolve.Client.Web.Controllers
{
    public class PuzzleController : Controller
    {
        private LibraryComponent compLib;

        public PuzzleController(LibraryComponent compLib)
        {
            this.compLib = compLib;
        }

        public IActionResult Home(string id)
        {
            var ident = PuzzleIdent.Parse(id);
            var p = compLib.GetPuzzleWithCaching(ident);
            return View(p);
        }
        
        public IActionResult Solve(string id)
        {
            var ident = PuzzleIdent.Parse(id);
            var p     = compLib.GetPuzzleWithCaching(ident);
            
            
            throw new NotImplementedException();
        }
    }
}