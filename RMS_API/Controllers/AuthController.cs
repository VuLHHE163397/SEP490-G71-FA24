﻿using Microsoft.AspNetCore.Identity;
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
                return BadRequest("Email không tồn tại.");
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

            var subject = "Verify user của bạn";
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
                return Unauthorized("Xin mời nhập tài khoản và mật khẩu");

            var user = await _context.Users
        .Include(u => u.Role)
        .SingleOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
                return Unauthorized("Tài khoản không tồn tại. Xin hãy đăng nhập lại!");
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                //if (user.Password != model.Password)
                return Unauthorized("Mật khẩu không đúng!");

            var token = GenerateJwtToken(user);

            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                //HttpOnly = true,
                Secure = true,
                Path = "/",
                HttpOnly = false,
                //Secure = false,
                //SameSite = SameSiteMode.Lax,
                SameSite=SameSiteMode.None,
                //SameSite = SameSiteMode.Strict, // Ngăn CSRF                
                Expires = DateTime.UtcNow.AddHours(1) 
            });

            Console.WriteLine("Đã gán AuthToken với HttpOnly = false");

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
        

            //var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.ASCII.GetBytes("Subjectcode_SoftwareProject490_Group71_Fall2024");

            //        var claims = new List<Claim>
            //{
            //    new Claim(ClaimTypes.Name, user.Email),
            //    new Claim("UserId", user.Id.ToString()) 
            //};

            //        if (user.Role != null && !string.IsNullOrEmpty(user.Role.Name))
            //        {
            //            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            //        }

            //        var tokenDescriptor = new SecurityTokenDescriptor
            //        {
            //            Subject = new ClaimsIdentity(claims),
            //            Expires = DateTime.UtcNow.AddHours(10),
            //            SigningCredentials = new SigningCredentials(
            //                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            //        };

            //        var token = tokenHandler.CreateToken(tokenDescriptor);
            //        return tokenHandler.WriteToken(token);
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
