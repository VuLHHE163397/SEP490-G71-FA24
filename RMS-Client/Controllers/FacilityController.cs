using Microsoft.AspNetCore.Mvc;

namespace RMS_Client.Controllers
{

    public class FacilityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ListFacility()
        {
            return View();
        }
        public IActionResult AddFacility()
        {
            return View();
        }
    }
}
