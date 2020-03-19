using Microsoft.AspNetCore.Mvc;
using SokoSolve.Core.Lib;

namespace SokoSolve.Client.Web.Controllers
{
    public class LibController : Controller
    {
        private readonly LibraryComponent compLib;

        public LibController(LibraryComponent compLib)
        {
            this.compLib = compLib;
        }

        // GET
        public IActionResult Index()
        {
            return View(compLib.GetCollection());
        }

        public IActionResult Home(string id)
        {
            var l = compLib.LoadLibraryRel(id);
            return View(l);
        }

        public IActionResult Puzzle(string lib, string puzzle)
        {
            throw new System.NotImplementedException();
        }
    }
}