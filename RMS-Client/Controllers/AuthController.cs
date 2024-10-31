using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static RMS_API.Controllers.AuthController;

namespace RMS_Client.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View("~/Views/Login/Login.cshtml");
        }

        public IActionResult Register()
        {
            return View("~/Views/Login/Register.cshtml");
        }

        [HttpPost]
        public IActionResult Login(UserLogin userLogin)
        {            
            return RedirectToAction("ListBuilding", "Building");
        }
    }

   
}
