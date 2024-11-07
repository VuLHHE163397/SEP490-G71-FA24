using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        [HttpPost]
        public async Task<IActionResult> AddRoom([FromBody] RoomLlDTO roomDTO)
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
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync(); // Đảm bảo dữ liệu được lưu trước khi trả về

            return Ok(room);
        }

        [HttpGet("GetActiveRooms")]
        public IActionResult GetActiveRooms()
        {
            var rooms = _context.Rooms
                .Where(r => r.RooomStatusId == 1) // Lọc các phòng có trạng thái đang hoạt động
                .Select(r => new
                {
                    Id = r.Id,
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

        [HttpGet("{roomId}/suggestedrooms")]
        public IActionResult GetSuggestedRooms(int roomId)
        {
            // Lấy thông tin phòng hiện tại để lấy BuildingId
            var currentRoom = _context.Rooms
                .FirstOrDefault(r => r.Id == roomId);

            if (currentRoom == null)
            {
                return NotFound("Phòng không tồn tại");
            }

            // Lấy danh sách các phòng gợi ý trong cùng BuildingId và RoomStatusId là 1 hoặc 4
            var suggestedRooms = _context.Rooms
                .Where(r => r.BuildingId == currentRoom.BuildingId &&
                            (r.RooomStatusId == 1 || r.RooomStatusId == 4) &&
                            r.Id != roomId) // Loại trừ phòng hiện tại
                .OrderBy(r => r.Price) // Sắp xếp theo giá tiền, nếu cần
                .Take(8) // Lấy top 8 phòng
                .Select(r => new SuggestedRoomDTO
                {
                    Id = r.Id,
                    Price = r.Price,
                    Area = r.Area,
                    RoomStatusName = r.RooomStatus.Name,
                    //Images = r.Images.Select(i => i.Url).ToList() // Giả sử có liên kết tới bảng Images
                })
                .ToList();

            return Ok(suggestedRooms);
        }
    }
}
