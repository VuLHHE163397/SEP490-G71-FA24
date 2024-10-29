using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RMS_API.Models;
using System.ComponentModel.DataAnnotations;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new User { Email = model.Email, FirstName = model.Firstname };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return Ok(new { result = "User registered successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password)) return Unauthorized();
            //var token = GenerateJwtToken(user);
            var token = "test";
            return Ok(new { token });

        }

        //private string GenerateJwtToken(User user)
        //{
        //    var claims = new[]
        //    {
        //    new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
        //    new Claim(JwtRegisteredClaimNames.Name, user.firstName),
        //    new Claim("firstName", user.firstName ?? String.Empty),
        //};

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["Jwt:Issuer"],
        //        audience: _configuration["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.Now.AddHours(24),
        //        signingCredentials: creds);

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
        public class RegisterModel
        {
            [Required]
            [DataType(DataType.Text)]
            public string Email { get; set; }
            [Required]
            [DataType(DataType.Text)]
            public string Firstname { get; set; }
            [Required]
            [DataType(DataType.Text)]
            public string Midname { get; set; }
            [Required]
            [DataType(DataType.Text)]
            public string Lastname { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public class LoginModel
        {
            [Required]
            [DataType(DataType.Text)]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }
    }
}
