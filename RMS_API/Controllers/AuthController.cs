using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.Models;
using System.Threading.Tasks;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public AuthController(RMS_SEP490Context context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User newUser)
        {
            if (_context.Users.Any(u => u.Email == newUser.Email))
            {
                return BadRequest("Email này đã được đăng ký!");
            }

            newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password); 
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
            {
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            // Create a response that includes user data or a token if using JWT authentication
            return Ok(new { message = "Đăng nhập thành công!", user = user });
        }

        public class UserLogin
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
