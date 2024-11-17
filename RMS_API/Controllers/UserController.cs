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
