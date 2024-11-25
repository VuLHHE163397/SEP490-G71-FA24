﻿using Microsoft.AspNetCore.Http;
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
                    RentedRooms = building.Rooms.Count(r => r.RoomStatus.Id != 1)  // Phòng cho thuê
                })
                .ToListAsync();

            return Ok(buildingsWithRoomStats);
        }
    }
}
