using Microsoft.AspNetCore.Mvc;

namespace RMS_Client.Controllers
{
    public class ServiceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ListService()
        {
            return View();
        }
        public IActionResult AddService()
        {
            return View();
        }
        public IActionResult EditService()
        {
            return View();
        }
    }
}
