using Microsoft.AspNetCore.Mvc;

namespace SokoSolve.Client.Web.Controllers
{
    public class SolveController : Controller
    {
        // GET
        public IActionResult Home()
        {
            return View();
        }
    }
}