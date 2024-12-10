
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using RMS_API.DTOs;
using System.Net;
using System.Net.Mail;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;
        private static Dictionary<string, string> _verificationCodes = new Dictionary<string, string>();
        private IConfiguration _configuration;
        public AuthController(RMS_SEP490Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Vui lòng nhập email.");
            }

            // Check if the email exists in the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return BadRequest("Email chưa được đăng ký.");
            }

            // Generate a random password
            string newPassword = GenerateRandomPassword();

            // Hash the new password and save it in the database
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            // Send email with new password
            var subject = "Mật khẩu mới của bạn";
            var body = $"Chúng tôi đã nhận được yêu cầu đổi mật khẩu cho tài khoản của bạn. Mật khẩu mới của bạn là: {newPassword}" +
                       $"\nVui lòng đăng nhập và thay đổi mật khẩu ngay sau khi đăng nhập.";

            if (SendEmail(user.Email, subject, body))
            {
                return Ok("Mật khẩu mới đã được gửi qua email của bạn.");
            }
            return StatusCode(500, "Không thể gửi email.");
        }

        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = null!;
        }


        // Helper method to generate a random password (letters and numbers)
        private string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789012345678901234567890123456789012345678901234567890123456789";
            Random rand = new Random();
            char[] password = new char[length];

            for (int i = 0; i < length; i++)
            {
                password[i] = validChars[rand.Next(validChars.Length)];
            }

            return new string(password);
        }


        [HttpPost("sendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode([FromBody] RegisterModel user)
        {

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Email này đã được đăng ký!");
            }

            var verificationCode = new Random().Next(100000, 999999).ToString();

            // Store the verification code in the dictionary
            _verificationCodes[user.Email] = verificationCode;

            var subject = "RMS: Xác thực tài khoản của bạn";
            var body = $"Mã code của bạn là: {verificationCode}." +
                $"\nXin không chia sẻ cho bất kỳ ai.";

            if (SendEmail(user.Email, subject, body))
            {
                return Ok("Code đã được gửi qua thư của bạn.");
            }
            return StatusCode(500, "Lỗi, không gửi đc qua thư của bạn.");
        }

        private bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential("thegalaxy2308@gmail.com", "vzls bayn firj pjzg");
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("thegalaxy2308@gmail.com"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false,
                    };
                    mailMessage.To.Add(toEmail);

                    smtpClient.Send(mailMessage);
                    return true;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi mail: {ex.Message}");
                return false;

            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            if (!_verificationCodes.ContainsKey(registerModel.Email))
            {
                return BadRequest("Xin vui lòng nhập mã code có 6 chữ số.");
            }

            if (_verificationCodes[registerModel.Email] != registerModel.VerificationCode)
            {
                return BadRequest("Code lỗi hoặc đã hết hạn. Mã code phải có 6 số.");
            }

            var user = new User
            {
                FirstName = registerModel.FirstName,
                MidName = registerModel.MidName,
                LastName = registerModel.LastName,
                Email = registerModel.Email,
                Phone = registerModel.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(registerModel.Password),
                UserStatusId = 1,
                RoleId = 2,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Remove the verification code after successful registration
            _verificationCodes.Remove(registerModel.VerificationCode);

            return Ok("Đăng ký thành công.");
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model.Email == null && model.Password == null)
                return Unauthorized("Xin mời nhập đủ tài khoản và mật khẩu");

            var user = await _context.Users
        .Include(u => u.Role)
        .SingleOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
                return Unauthorized("Không tìm thấy tài khoản. Vui lòng nhập lại!");
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                //if (user.Password != model.Password)
                return Unauthorized("Mật khẩu không đúng!");

            if (user.UserStatusId == 3)
            {
                return Unauthorized("Tài khoản đã bị cấm. Vui lòng dùng tài khoản khác!");
            }

            var token = GenerateJwtToken(user);

            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                //HttpOnly = true,
                Secure = true,
                Path = "/",
                HttpOnly = false,
                //Secure = false,
                //SameSite = SameSiteMode.Lax,
                SameSite = SameSiteMode.None,
                //SameSite = SameSiteMode.Strict, // Ngăn CSRF                
                Expires = DateTime.UtcNow.AddHours(1)
            });           

            HttpContext.Session.SetString("UserId", user.Id.ToString());

            return Ok(new { token });
        }

        public static (string FirstName, string MiddleName, string LastName) SplitName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (string.Empty, string.Empty, string.Empty);
            }

            // Loại bỏ khoảng trắng thừa
            fullName = fullName.Trim();
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                // Trường hợp chỉ có 1 từ
                return (parts[0], string.Empty, string.Empty);
            }
            else if (parts.Length == 2)
            {
                // Trường hợp có 2 từ
                return (parts[0], string.Empty, parts[1]);
            }
            else
            {
                // Trường hợp có 3 từ trở lên
                var firstName = parts[0]; // Từ đầu tiên
                var lastName = parts[^1]; // Từ cuối cùng
                var middleName = string.Join(" ", parts[1..^1]); // Các từ ở giữa

                return (firstName, middleName, lastName);
            }
        }


        [HttpPost("LoginByGoogle")]
        public async Task<IActionResult> LoginByGoogles([FromBody] LoginByGoogle model)
        {


            if (model.Email == null || model.Name == null)
                return Unauthorized("Xin mời nhập tài khoản và mật khẩu");

            var user = await _context.Users
        .Include(u => u.Role)
        .SingleOrDefaultAsync(u => u.Email == model.Email);
            var (firstName, middleName, lastName) = SplitName(model.Name);
            if (user == null)
            {
                var User = new User
                {
                    Phone = "0123456789",
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                    UserStatusId = 1,
                    Email = model.Email,
                    FirstName = firstName,
                    MidName = middleName,
                    LastName = lastName,
                    RoleId = 2,
                    Role = new Role { Name = "Landlord" }
                };
                _context.Users.Add(User);
                _context.SaveChanges();
                user = User;

            }




            var token = GenerateJwtToken(user);

            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                //HttpOnly = true,
                Secure = true,
                Path = "/",
                HttpOnly = false,
                //Secure = false,
                //SameSite = SameSiteMode.Lax,
                SameSite = SameSiteMode.None,
                //SameSite = SameSiteMode.Strict, // Ngăn CSRF                
                Expires = DateTime.UtcNow.AddHours(3)
            });      

            HttpContext.Session.SetString("UserId", user.Id.ToString());

            return Ok(new { token });
        }




        //Gen token for jwt
        private string GenerateJwtToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, userInfo.Email));
            claims.Add(new Claim("UserId", userInfo.Id.ToString()));
            claims.Add(new Claim("Roles", userInfo.Role.Name));

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        //Model login
        public class LoginByGoogle
        {
            [Required]
            [DataType(DataType.Text)]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Name { get; set; } = string.Empty;
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