using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PROG455_FinalProject_ExerciseTracker.Models;
using System.Diagnostics;

namespace PROG455_FinalProject_ExerciseTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly string BaseUrl = "http://ec2-18-223-162-6.us-east-2.compute.amazonaws.com/";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var dict = new Dictionary<DateTime, Tuple<int, int>>
            {
                {DateTime.Today, new(3, 15) }
            };

            var json = JsonConvert.SerializeObject(dict);

            var id = Hasher.CreateID();

            HttpContext.Session.SetString("API", BaseUrl);
            return View();
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
