using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

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
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Email này đã được đăng ký!");
            }

            var newUser = new User
            {
                Email = user.Email,
                FirstName = user.FirstName,
                MidName = user.MidName,
                LastName = user.LastName,
                Phone = user.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
                RoleId = 2,
                UserStatusId = 1
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đăng ký thành công!" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users
        .Include(u => u.Role)
        .SingleOrDefaultAsync(u => u.Email == model.Email);

           
            if (user == null)
                return Unauthorized("Tài khoản không tồn tại. Xin hãy đăng nhập lại!");
            if ( !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            //if (user.Password != model.Password)
                return Unauthorized("Mật khẩu không đúng!");


            var token = GenerateJwtToken(user);
            return Ok(new { token });            
        }

        //Gen token for jwt
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("Subjectcode_SoftwareProject490_Group71_Fall2024"); // Replace with a secure key

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
            };

            if (user.Role != null && !string.IsNullOrEmpty(user.Role.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            //    var tokenDescriptor = new SecurityTokenDescriptor
            //    {
            //        Subject = new ClaimsIdentity(new[]
            //        {
            //    new Claim(ClaimTypes.Name, user.Email),
            //    new Claim(ClaimTypes.Role, user.Role.Name)
            //}),
            //        Expires = DateTime.UtcNow.AddHours(1),
            //        SigningCredentials = new SigningCredentials(
            //            new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            //    };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        //Model register
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
            [DataType(DataType.Text)]
            public string Phone { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        //Model login
        public class LoginModel
        {
            [Required]
            [DataType(DataType.Text)]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }
    }
}
