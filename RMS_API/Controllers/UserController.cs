using Microsoft.AspNetCore.Mvc;
using RMS_API.DTOs;
using System.Threading.Tasks;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("getProfile")]
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {
            // Here, retrieve user info from the database based on the provided email
            // This is a placeholder; replace it with your database call to get user details
            var userProfile = new RegisterModel
            {
                FirstName = "John",
                LastName = "Doe",
                MidName = "A",
                Email = email,
                Phone = "1234567890"
                // Populate other fields as necessary
            };

            return Ok(userProfile);
        }
    }
}
