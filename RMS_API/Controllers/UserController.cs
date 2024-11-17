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

            // Tìm user theo Id
            var user = await _context.Users
                .Include(u => u.Buildings) // Include Buildings liên quan
                .FirstOrDefaultAsync(u => u.Id == request.Id);

            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Xác thực trạng thái UserStatus
            var userStatus = await _context.UserStatuses.FindAsync(request.NewStatusId);
            if (userStatus == null)
            {
                return NotFound(new { Message = "User status not found." });
            }

            // Cập nhật UserStatus
            user.UserStatusId = request.NewStatusId;

            // Map UserStatusId sang BuildingStatusId
            int newBuildingStatusId = MapUserStatusToBuildingStatus(request.NewStatusId);

            // Cập nhật BuildingStatus cho tất cả Buildings của User này
            foreach (var building in user.Buildings)
            {
                building.BuildingStatusId = newBuildingStatusId;
            }

            // Lưu thay đổi
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Trạng thái người dùng và các trạng thái tòa nhà liên quan được cập nhật thành công." });
        }

        private int MapUserStatusToBuildingStatus(int userStatusId)
        {
            // Logic ánh xạ UserStatusId -> BuildingStatusId
            // Ví dụ:
            switch (userStatusId)
            {
                case 1: return 1; // UserStatusId 1 -> BuildingStatusId 1
                case 2: return 2; // UserStatusId 2 -> BuildingStatusId 2
                case 3: return 2; // UserStatusId 3 -> BuildingStatusId 3
                default: return 0; // Mặc định là 0 nếu không có ánh xạ
            }

        }
    }
}
