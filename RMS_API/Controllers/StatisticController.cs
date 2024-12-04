using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.DTOs;
using RMS_API.Helper;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public StatisticController(RMS_SEP490Context context)
        {
            _context = context;
        }
        [HttpGet("room-stats")]
        public async Task<ActionResult> GetRoomStats()
        {
            string token = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(token)) return BadRequest("Invalid token");
            if (token.StartsWith("Bearer "))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }
            int? userId = UserHelper.GetUserIdFromToken(token);
            //if (int.IsNullOrEmpty(userId)) return Unauthorized("Invalid User");
            if (userId == null)
            {
                return BadRequest("User not exsit");
            }
            var buildingsWithRoomStats = await _context.Buildings
                .Include(b => b.Rooms)
                .ThenInclude(r => r.RoomStatus)
                .Where(b => b.UserId == userId)
                .Select(building => new RoomStatsDTO
                {
                    BuildingName = building.Name,
                    AvailableRooms = building.Rooms.Count(r => r.RoomStatus.Id == 1),  // Phòng trống
                    RentedRooms = building.Rooms.Count(r => r.RoomStatus.Id == 2),  // Phòng cho thuê
                    MaintenanceRooms = building.Rooms.Count(r => r.RoomStatus.Id == 3),//Phòng sắp trống
                    ExpiringRooms = building.Rooms.Count(r => r.RoomStatus.Id == 4)//Phòng đang bảo trì
                })
                .ToListAsync();

            return Ok(buildingsWithRoomStats);
        }

        // 1. API đếm tổng số phòng
        [HttpGet("count-rooms")]
        public IActionResult CountRooms()
        {
            try
            {
                int totalRooms = _context.Rooms.Count(); // Assuming 'Rooms' is the DbSet for rooms
                return Ok(new { totalRooms });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while counting rooms.", error = ex.Message });
            }
        }

        // API đếm tổng số người dùng có roleId = 2
        [HttpGet("count-users-with-role2")]
        public IActionResult CountUsersWithRole2()
        {
            try
            {
                int totalUsersWithRole2 = _context.Users.Count(u => u.RoleId == 2);
                return Ok(new { totalUsers = totalUsersWithRole2 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while counting users with roleId = 2.", error = ex.Message });
            }
        }

        // 3. API đếm tổng số tòa nhà
        [HttpGet("count-buildings")]
        public IActionResult CountBuildings()
        {
            try
            {
                int totalBuildings = _context.Buildings.Count(); // Assuming 'Buildings' is the DbSet for buildings
                return Ok(new { totalBuildings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while counting buildings.", error = ex.Message });
            }
        }
    }
}
