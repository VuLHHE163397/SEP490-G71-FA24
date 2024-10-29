using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RMS_Client.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Security.Claims;

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
            return View("Views/Login/Login.cshtml");
        }

        public IActionResult Register()
        {
            return View("Views/Login/Register.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}
