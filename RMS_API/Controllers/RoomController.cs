using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.DTOs;
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

        [HttpGet("GetAllStatus")]
        public IActionResult GetAllStatus()
        {
            var ro = _context.RoomStatuses.ToList();
            return Ok(ro);
        }

        [HttpGet("GetAllBuilding")]
        public IActionResult GetAllBuilding()
        {
            var bui = _context.Buildings.ToList();
            return Ok(bui);
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

        [HttpPost]
        public async Task<IActionResult> AddRoom([FromBody] RoomDTO roomDTO)
        {
            // Kiểm tra sự tồn tại của Building
            var building = await _context.Buildings.FindAsync(roomDTO.BuildingId);
            if (building == null)
            {
                return BadRequest("BuildingId không tồn tại.");
            }

            // Kiểm tra sự tồn tại của RoomStatus
            var roomStatus = await _context.RoomStatuses.FindAsync(roomDTO.RoomStatusId);
            if (roomStatus == null)
            {
                return BadRequest("RoomStatusId không tồn tại.");
            }

            // Kiểm tra RoomNumber trong cùng Building có bị trùng không
            var existingRoom = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomNumber == roomDTO.RoomNumber && r.BuildingId == roomDTO.BuildingId);

            if (existingRoom != null)
            {
                return BadRequest("RoomNumber đã tồn tại trong Building này.");
            }

            // Kiểm tra RoomNumber bắt đầu bằng số tầng
            int floorPrefix = int.Parse(roomDTO.RoomNumber.ToString()[0].ToString());
            if (floorPrefix != roomDTO.Floor)
            {
                return BadRequest("RoomNumber phải bắt đầu bằng số tầng.");
            }

            // Tạo đối tượng Room mới từ DTO
            var room = new Room
            {
                Price = roomDTO.Price,
                RoomNumber = roomDTO.RoomNumber,
                Area = roomDTO.Area,
                Description = roomDTO.Description,
                Floor = roomDTO.Floor,
                StartedDate = roomDTO.StartedDate,
                ExpiredDate = roomDTO.ExpiredDate,
                BuildingId = roomDTO.BuildingId,
                RooomStatusId = roomDTO.RoomStatusId,
                ServiceId = roomDTO.ServiceId
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync(); // Đảm bảo dữ liệu được lưu trước khi trả về

            return Ok(room);
        }

    }
}
