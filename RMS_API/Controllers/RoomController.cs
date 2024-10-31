using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.Models;


namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public RoomController(RMS_SEP490Context context)
        {
            _context = context;
        }
        [HttpGet("GetAllRoom")]
        public IActionResult GetAllRoom()
        {
            var ro = _context.Rooms.ToList();
            return Ok(ro);
        }

        [HttpGet("GetRoomByBuilding")]
        public IActionResult GetRoomByBuilding(int buildingId)
        {
            var room = _context.Rooms.Where(p => p.BuildingId == buildingId).ToList();
            return Ok(room);
        }

        [HttpGet("GetRoomByStatus")]
        public IActionResult GetRoomByStatus(int statusId)
        {
            var room = _context.Rooms.Where(p => p.RooomStatusId == statusId).ToList();
            return Ok(room);
        }

        [HttpGet("GetActiveRooms")]
        public IActionResult GetActiveRooms()
        {
            var rooms = _context.Rooms
                .Where(r => r.RooomStatusId == 1) // Lọc các phòng có trạng thái đang hoạt động
                .Select(r => new
                {
                    Distance = r.Building.Distance,
                    Address = $"{r.Building.Address.Information}, {r.Building.Address.Ward.Name}, {r.Building.Address.District.Name}, {r.Building.Address.Province.Name}",
                    Price = r.Price,
                    Area = r.Area,
                    RoomStatusName = r.RooomStatus.Name
                })
                .ToList();

            return Ok(rooms);
        }

    }
}
