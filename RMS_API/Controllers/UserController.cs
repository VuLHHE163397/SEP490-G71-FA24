using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.DTOs;
using RMS_API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public UserController(RMS_SEP490Context context)
        {
            _context = context;
        }

        [HttpGet("GetUserByEmail")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }

            var user = await _context.Users               
               .Where(u => u.Email == email)
               .Select(b => new RegisterModel
               {                   
                   FirstName = b.FirstName,
                   LastName = b.LastName,
                   MidName = b.MidName,
                   Email = b.Email,
                   Phone = b.Phone                  
               })
               .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Building not found.");
            }
            return Ok(user);
        }

        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromQuery] string email, [FromQuery] string currentPassword, [FromQuery] string newPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                return BadRequest("Email hoặc current password và new password không được trống.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.Password != currentPassword) // Adjust according to your password hashing method if applicable
            {
                return BadRequest("Current password is incorrect.");
            }

            user.Password = newPassword; // Hash the password if needed
            await _context.SaveChangesAsync();

            return Ok("Password updated successfully.");
        }

        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] RegisterModel userDTO)
        {
            if (userDTO == null || string.IsNullOrEmpty(userDTO.Email))
            {
                return BadRequest("Invalid user data.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDTO.Email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.MidName = userDTO.MidName;
            user.Phone = userDTO.Phone;

            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully.");
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromQuery] string resetToken, [FromQuery] string newPassword)
        {
            if (string.IsNullOrEmpty(resetToken) || string.IsNullOrEmpty(newPassword))
            {
                return BadRequest("Token and new password are required.");
            }

            //var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == resetToken && u.ResetTokenExpiry > DateTime.UtcNow);
            var user= new RegisterModel();
            if (user == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            // Update password and clear the reset token
            user.Password = newPassword; // Hash the password if needed
            //user.ResetToken = null;
            //user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok("Password has been reset successfully.");
        }
        [HttpGet("GetAllUserByRoleId")]
        public async Task<IActionResult> GetAllUserByRoleId(int roleId)
        {
            // Retrieve all users with the specified RoleId
            var users = await _context.Users
                .Where(u => u.RoleId == roleId)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MidName = u.MidName,
                    Email = u.Email,
                    Password = u.Password,
                    Phone = u.Phone,
                    UserStatusId = u.UserStatusId,
                    RoleId = u.RoleId
                })
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound($"No users found with RoleId: {roleId}");
            }

            return Ok(users);
        }


        [HttpGet("GetAllLanlord")]
        public async Task<IActionResult> GetAllLanlord()//Lanlord se co roleid =2
        {
            // Retrieve all users with RoleId = 2
            var users = await _context.Users
                .Where(u => u.RoleId == 2)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MidName = u.MidName,
                    Email = u.Email,
                    Password = u.Password,
                    Phone = u.Phone,
                    UserStatusId = u.UserStatusId,
                    RoleId = u.RoleId,
                    UserStatusName = u.UserStatus.Name,
                    RoleName = u.Role.Name
                })
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound("No users found with RoleId: 2");
            }

            return Ok(users);
        }


        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            if (request == null || request.Id <= 0 || request.NewStatusId <= 0)
            {
                return BadRequest(new { Message = "Invalid input data." });
            }

            // Find the user by Id
            var user = await _context.Users.FindAsync(request.Id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Find the status by the new status ID to validate it exists
            var status = await _context.UserStatuses.FindAsync(request.NewStatusId);
            if (status == null)
            {
                return NotFound(new { Message = "Status not found." });
            }

            // Update the user's status
            user.UserStatusId = request.NewStatusId;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User status updated successfully." });
        }

    }
}
