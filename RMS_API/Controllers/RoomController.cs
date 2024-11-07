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

        [HttpGet("GetRoomById/{roomId}")]
        public IActionResult GetAllRoom(int roomId)
        {
            var ro = _context.Rooms.FirstOrDefault(p => p.Id == roomId);
            return Ok(ro);
        }

        [HttpGet("GetRoomByBuilding")]
        public IActionResult GetRoomByBuilding(int buildingId)
        {
            var room = _context.Rooms.Where(p => p.BuildingId == buildingId).ToList();
            return Ok(room);
        }

        [HttpGet("GetBuildingById")]
        public IActionResult GetBuildingNameById(int buildingId)
        {
            var buildingName = _context.Buildings
                                       .Where(p => p.Id == buildingId)
                                       .Select(p => p.Name)
                                       .FirstOrDefault();

            if (buildingName == null)
            {
                return NotFound("Building not found.");
            }

            return Ok(buildingName);
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

        [HttpPost("AddRoom")]
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
                RoomStatusId = roomDTO.RoomStatusId,
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync(); // Đảm bảo dữ liệu được lưu trước khi trả về

            return Ok(room);
        }

        [HttpDelete("DeleteRoomById/{roomId}")]
        public IActionResult DeteleRoomById(int roomId)
        {
            // Kiểm tra nếu phòng không tồn tại
            var room = _context.Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null)
            {
                return NotFound($"Room with ID {roomId} not found.");
            }

            // Xóa các liên kết dữ liệu liên quan đến phòng (Facilities, RoomHistories, ServicesOfRooms, Tennants)
            var facilities = _context.Facilities.Where(f => f.RoomId == roomId);
            _context.Facilities.RemoveRange(facilities);

            var roomHistories = _context.RoomHistories.Where(h => h.RoomId == roomId);
            _context.RoomHistories.RemoveRange(roomHistories);

            var servicesOfRooms = _context.ServicesOfRooms.Where(s => s.RoomId == roomId);
            _context.ServicesOfRooms.RemoveRange(servicesOfRooms);

            var tenants = _context.Tennants.Where(t => t.RoomId == roomId);
            _context.Tennants.RemoveRange(tenants);

            // Xóa phòng
            _context.Rooms.Remove(room);

            // Lưu thay đổi vào database
            _context.SaveChanges();

            return NoContent(); // Trả về 204 No Content sau khi xóa thành công
        }

        [HttpDelete("DeleteAllRoom/{buildingId}")]
        public IActionResult DeleteAllRoomByBuildingId(int buildingId)
        {
            // Kiểm tra xem có phòng nào thuộc tòa nhà với buildingId không
            var rooms = _context.Rooms.Where(r => r.BuildingId == buildingId).ToList();
            if (!rooms.Any())
            {
                return NotFound("Không tìm thấy phòng nào thuộc tòa nhà có ID = " + buildingId);
            }

            // Xóa các đối tượng liên quan đến phòng
            foreach (var room in rooms)
            {
                // Xóa Facilities liên quan đến room
                var facilities = _context.Facilities.Where(f => f.RoomId == room.Id);
                _context.Facilities.RemoveRange(facilities);

                // Xóa RoomHistories liên quan đến room
                var roomHistories = _context.RoomHistories.Where(h => h.RoomId == room.Id);
                _context.RoomHistories.RemoveRange(roomHistories);

                // Xóa ServicesOfRooms liên quan đến room
                var servicesOfRooms = _context.ServicesOfRooms.Where(s => s.RoomId == room.Id);
                _context.ServicesOfRooms.RemoveRange(servicesOfRooms);

                // Xóa Tennants liên quan đến room
                var tenants = _context.Tennants.Where(t => t.RoomId == room.Id);
                _context.Tennants.RemoveRange(tenants);
            }

            // Cuối cùng, xóa các phòng
            _context.Rooms.RemoveRange(rooms);

            // Lưu thay đổi vào database
            _context.SaveChanges();

            return Ok("Đã xóa tất cả phòng thuộc tòa nhà có ID = " + buildingId);
        }


        [HttpGet("GetActiveRooms")]
        public IActionResult GetActiveRooms()
        {
            var rooms = _context.Rooms
                .Where(r => r.RoomStatusId == 1) // Lọc các phòng có trạng thái đang hoạt động
                .Select(r => new
                {
                    Id = r.Id,
                    Distance = r.Building.Distance,
                    Address = $"{r.Building.Address.Information}, {r.Building.Address.Ward.Name}, {r.Building.Address.District.Name}, {r.Building.Address.Province.Name}",
                    Price = r.Price,
                    Area = r.Area,
                    RoomStatusName = r.RoomStatus.Name,
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
                .Include(r => r.RoomStatus)
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
                RoomStatus = room.RoomStatus?.Name ?? "Trạng thái không xác định",
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
