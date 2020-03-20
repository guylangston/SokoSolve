using Microsoft.AspNetCore.Mvc;

namespace SokoSolve.Client.Web.Controllers
{
    public class PuzzleController : Controller
    {
        // GET
        public IActionResult Home(string id)
        {
            return View();
        }
    }
}