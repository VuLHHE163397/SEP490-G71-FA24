using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using RMS_Client.Models;
using System.Diagnostics;

namespace RMS_Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Home()
        {
            return View();
        }
        public IActionResult ListRoom()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View("~/Views/Login/Login.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
