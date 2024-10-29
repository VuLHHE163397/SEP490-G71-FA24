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

        [HttpGet("ListRoom")]
        public async Task<ActionResult<IEnumerable<Room>>> GetAvailableRooms()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Building) // Đưa thông tin về Building
                .Include(b => b.Building.Address) // Đưa thông tin về Address
                .Include(r => r.RooomStatus) // Đưa thông tin về RoomStatus
                .Where(r => r.RooomStatusId == 1) // Lọc phòng đang trống
                .ToListAsync();

            return Ok(rooms);
        }



    }
}
