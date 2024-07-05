using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PROG455_FinalProject_ExerciseTracker.Models;
using System.Diagnostics;
using System.Security.Principal;

namespace PROG455_FinalProject_ExerciseTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly string BaseUrl = "http://amazonaws.com/";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // TODO: FINISH SECURE HOMECONTROLLER login
        public IActionResult Index()
        {
            HttpContext.Session.SetString("API", BaseUrl);
            return RedirectToAction("Index", "User");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
