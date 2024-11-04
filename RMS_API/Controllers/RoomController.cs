using Microsoft.AspNetCore.Http;
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

        [HttpGet("GetActiveRooms")]
        public IActionResult GetActiveRooms()
        {
            var rooms = _context.Rooms
                .Where(r => r.RooomStatusId == 1) // Lọc các phòng có trạng thái đang hoạt động
                .Select(r => new
                {   
                    Id=r.Id,
                    Distance = r.Building.Distance,
                    Address = $"{r.Building.Address.Information}, {r.Building.Address.Ward.Name}, {r.Building.Address.District.Name}, {r.Building.Address.Province.Name}",
                    Price = r.Price,
                    Area = r.Area,
                    RoomStatusName = r.RooomStatus.Name,
                    //Images = r.Images.Select(i => i.Link).ToList()
                })
                .ToList();

            return Ok(rooms);
        }

        [HttpGet("detail/{id}")]
        public IActionResult GetRoomDetail(int id)
        {
            // Lấy thông tin phòng theo ID
            var room = _context.Rooms
                .Include(r => r.Building)
                    .ThenInclude(b => b.Address)
                .Include(r => r.Building)
                    .ThenInclude(b => b.Ward)
                .Include(r => r.Building)
                    .ThenInclude(b => b.District)
                .Include(r => r.Building)
                    .ThenInclude(b => b.Province)
                .Include(r => r.RooomStatus)
                .Include(r => r.Building.User) // Lấy thông tin chủ nhà
                .FirstOrDefault(r => r.Id == id);

            if (room == null)
            {
                return NotFound(); // Không tìm thấy phòng
            }

            // Tạo RoomDetailDto từ dữ liệu phòng
            var roomDetailDto = new RoomDetailDTO
            {
                FullAddress = $"{room.Building?.Address?.Information ?? "Chưa có địa chỉ chi tiết"}, " +
                              $"{room.Building?.Ward?.Name ?? "Chưa có phường"}, " +
                              $"{room.Building?.District?.Name ?? "Chưa có quận"}, " +
                              $"{room.Building?.Province?.Name ?? "Chưa có tỉnh"}",
                Price = room.Price,
                Area = room.Area,
                Distance = room.Building?.Distance ?? 0,
                Description = room.Description,
                RoomStatus = room.RooomStatus?.Name ?? "Trạng thái không xác định",
                OwnerName = $"{room.Building?.User?.LastName ?? ""} " +
                            $"{room.Building?.User?.MidName ?? ""} " +
                            $"{room.Building?.User?.FirstName ?? ""}".Trim(),
                OwnerPhone = room.Building?.User?.Phone ?? "Số điện thoại không có sẵn",
                LinkEmbedMap = room.Building?.LinkEmbedMap ?? "Không có liên kết bản đồ",
                // ImageUrl = room.ImageUrl // Thêm trường ảnh nếu có
            };

            return Ok(roomDetailDto);
        }

    }
}
