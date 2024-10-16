using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RMS_Client.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class BuildingController : Controller
    {
        private readonly ILogger<BuildingController> _logger;

        public BuildingController(ILogger<BuildingController> logger)
        {
            _logger = logger;
        }

        public IActionResult ListBuilding()
        {
           return View("~/Views/ManageBuilding/ListBuilding.cshtml");
        }

    }
}
