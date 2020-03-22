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
            return View(compLib.GetDefaultLibraryCollection());
        }

        public IActionResult Home(string id)
        {
            var l = compLib.GetLibraryWithCaching(id);
            return View(l);
        }

       
    }
}