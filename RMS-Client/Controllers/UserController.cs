using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RMS_Client.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View("~/Views/Login/Login.cshtml");
        }
    }
}
