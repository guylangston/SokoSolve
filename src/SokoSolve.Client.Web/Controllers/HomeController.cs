using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SokoSolve.Client.Web.Logic;
using SokoSolve.Client.Web.Models;

namespace SokoSolve.Client.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GraphVisWrapper wrapGraphVis;

        public HomeController(ILogger<HomeController> logger, GraphVisWrapper wrapGraphVis)
        {
            _logger           = logger;
            this.wrapGraphVis = wrapGraphVis;
        }

        public IActionResult Index()
        {
            return View();
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }


        public async Task<IActionResult> Test() 
            => await wrapGraphVis.RenderToActionResult("digraph{a -> b; b -> c; c -> a;}");

    }


    
}