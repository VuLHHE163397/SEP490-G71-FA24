using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email Không được để trống.");
            }

            var user = await _context.Users               
               .Where(u => u.Email == email)
               .Select(b => new ProfileDTO
               {
                   Id = b.Id,
                   FirstName = b.FirstName,
                   LastName = b.LastName,
                   MidName = b.MidName,
                   Email = b.Email,
                   Phone = b.Phone,
                   FacebookUrl = b.FacebookUrl,
                   ZaloUrl = b.ZaloUrl
               })
               .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }
            return Ok(user);
        }


        [HttpGet("GetUserById")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> GetUserById([FromQuery] int id)
        {
            if (id == null)
            {
                return BadRequest("Id Không được để trống.");
            }

            var user = await _context.Users
               .Where(u => u.Id == id)
               .Select(b => new ProfileDTO
               {
                   Id = b.Id,
                   FirstName = b.FirstName,
                   LastName = b.LastName,
                   MidName = b.MidName,
                   Email = b.Email,
                   Phone = b.Phone,
                   FacebookUrl = b.FacebookUrl,
                   ZaloUrl = b.ZaloUrl,
                   UserStatusId = b.UserStatusId
               })
               .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }
            return Ok(user);
        }


        [HttpGet("GetUserNameById")]        
        public async Task<IActionResult> GetUserNameById([FromQuery] int id)
        {
            if (id == 0) // Kiểm tra ID
            {
                return BadRequest("Id không được để trống.");
            }

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(b => new UserNameDTO
                {                    
                    FullName = $"{b.LastName} {b.MidName} {b.FirstName}".Trim(), // Gộp họ tên                    
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            return Ok(user);
        }

        [HttpPut("ChangePassword")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> ChangePassword([FromQuery] string email, [FromQuery] string currentPassword, [FromQuery] string newPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                return BadRequest("Email,mật khẩu hiện tại và mật khẩu mới không được trống.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return NotFound("Email không tồn tại.");
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password)) // Adjust according to your password hashing method if applicable
            {
                return BadRequest("Mật khẩu hiện tại sai.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword); // Hash the password if needed
            await _context.SaveChangesAsync();

            return Ok("Cập nhật mật khẩu thành công.");
        }

        [HttpPut("UpdateProfileByEmail")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> UpdateProfileByEmail([FromBody] ProfileDTO userDTO)
        {
            if (userDTO == null || string.IsNullOrEmpty(userDTO.Email))
            {
                return BadRequest("Không tìm thấy người dùng.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDTO.Email);

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.MidName = userDTO.MidName;
            user.Phone = userDTO.Phone;
            user.FacebookUrl = userDTO.FacebookUrl;
            user.ZaloUrl = userDTO.ZaloUrl;

            await _context.SaveChangesAsync();

            return Ok("Cập nhật profile thành công.");
        }

        [HttpPut("UpdateProfile")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileDTO userDTO)
        {
            if (userDTO == null)
            {
                return BadRequest("Không tìm thấy người dùng.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userDTO.Id);

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.MidName = userDTO.MidName;
            user.Phone = userDTO.Phone;
            user.FacebookUrl = userDTO.FacebookUrl;
            user.ZaloUrl = userDTO.ZaloUrl;

            await _context.SaveChangesAsync();

            return Ok("Cập nhật profile thành công.");
        }


        [HttpPost("ChangePassword")]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            if (model == null || string.IsNullOrEmpty(model.OldPassword) || string.IsNullOrEmpty(model.NewPassword))
            {
                return BadRequest("Vui lòng nhập đầy đủ thông tin.");
            }

            // Find the user based on the token or authentication context
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == model.UserId); // Adjust as per your user identification method
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            // Verify the old password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password))
            {
                return BadRequest("Mật khẩu cũ không đúng.");
            }

            // Hash the new password using BCrypt and update it
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok("Thay đổi mật khẩu thành công.");
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
            //var status = await _context.UserStatuses.FindAsync(request.NewStatusId);
            //if (status == null)
            //{
            //    return NotFound(new { Message = "Status not found." });
            //}

            // Update the user's status
            user.UserStatusId = request.NewStatusId;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User status updated successfully." });
        }


        public class UserNameDTO
        {
            public string FullName { get; set; }
        }
    }
}
