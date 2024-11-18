using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using RMS_API.DTOs;
using RMS_API.Models;
using System.Net.NetworkInformation;
using System.Security.Principal;
using static System.Net.Mime.MediaTypeNames;


namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;
        public const string cloudName = "dzqken1lp";
        public const string apiKey = "786234759781774";
        public const string apiSecret = "KBsJPM9AD-hVf8sCOkCkjeoWtHs";
        private readonly Cloudinary _cloudinary;

        public RoomController(RMS_SEP490Context context)
        {
            Account account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);

            _context = context;
        }

        [HttpGet("GetAllRoom")]
        public IActionResult GetAllRoom()
        {
            var ro = _context.Rooms.ToList();
            return Ok(ro);
        }

        [HttpGet("GetRoomById/{roomId}")]
        public IActionResult GetRoomById(int roomId)
        {
            var ro = _context.Rooms.FirstOrDefault(p => p.Id == roomId);
            return Ok(ro);
        }

        [HttpGet("GetRoomByBuilding/{buildingId}")]
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

        [HttpGet("GetAllImage/{roomId}")]
        public IActionResult GetImageByRoom(int roomId)
        {
            var image = _context.Images.Where(p => p.RoomId == roomId).ToList();
            return Ok(image);
        }

        [HttpPost("ImportRooms")]
        public async Task<IActionResult> ImportRooms(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var room = new Room
                        {
                            RoomNumber = worksheet.Cells[row, 1].Text,
                            Area = double.Parse(worksheet.Cells[row, 2].Text),
                            Floor = int.Parse(worksheet.Cells[row, 3].Text),
                            Price = decimal.Parse(worksheet.Cells[row, 4].Text),
                            RoomStatusId = int.Parse(worksheet.Cells[row, 5].Text),
                            Description = worksheet.Cells[row, 6].Text,
                            StartedDate = DateTime.Parse(worksheet.Cells[row, 7].Text),
                            ExpiredDate = DateTime.Parse(worksheet.Cells[row, 8].Text),
                            FreeInFutureDate = DateTime.Parse(worksheet.Cells[row, 9].Text),

                        };

                        _context.Rooms.Add(room);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return Ok("Rooms imported successfully.");
        }



        //[HttpPost("UploadImage/{roomId}")]
        //public async Task<IActionResult> UploadImage(int roomId, [FromBody] ImageDTO imageDto)
        //{
        //    if (imageDto == null || string.IsNullOrEmpty(imageDto.Link))
        //    {
        //        return BadRequest("Đường link ảnh không hợp lệ.");
        //    }

        //    // Lưu imageDto vào cơ sở dữ liệu
        //    var room = await _context.Rooms.FindAsync(roomId);
        //    if (room == null)
        //    {
        //        return NotFound("Room không tồn tại.");
        //    }

        //    var image = new Models.Image
        //    {
        //        Link = imageDto.Link,
        //        RoomId = roomId
        //    };

        //    _context.Images.Add(image);
        //    await _context.SaveChangesAsync();

        //    return Ok("Ảnh đã được lưu thành công.");
        //}

        [HttpPost]
        public async Task<IActionResult> UploadImageToCloudinary(IFormFile imageFile, int roomId)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("Image", "Vui lòng chọn một ảnh.");
            }

            // Thiết lập tham số upload cho Cloudinary
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream()),
                Folder = "RoomImages"
            };

            // Upload ảnh lên Cloudinary
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // Lấy link ảnh từ Cloudinary và lưu vào database
                var imageUrl = uploadResult.SecureUrl.ToString();
                var image = new Models.Image
                {
                    Link = imageUrl,
                    RoomId = roomId
                };
                _context.Images.Add(image);
                await _context.SaveChangesAsync();

                return Ok(new { imageUrl = imageUrl });  // Trả về link ảnh sau khi upload thành công

            }
            else
            {
                return StatusCode(500, "Lỗi khi tải ảnh lên Cloudinary.");
            }
        }

        [HttpDelete("DeleteImageById/{id}")]
        public IActionResult DeleteImageById(int id)
        {
            var image = _context.Images.FirstOrDefault(r => r.Id == id);
            if (image == null)
            {
                return NotFound(new { message = $"Image with ID {id} not found." });
            }

            _context.Images.Remove(image);
            _context.SaveChanges();

            return NoContent();
        }


        [HttpGet("GetFacilityByRoomId/{roomId}")]
        public IActionResult GetAllFacilities(int roomId)
        {
            // Lấy danh sách các cơ sở vật chất cho RoomId thay vì chỉ một phần tử
            var fa = _context.Facilities.Where(p => p.RoomId == roomId).ToList();
            return Ok(fa);
        }

        [HttpPost("AddRoom")]
        public async Task<IActionResult> AddRoom([FromBody] RoomLlDTO roomDTO)
        {

            // Kiểm tra RoomNumber trong cùng Building có bị trùng không
            var existingRoom = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomNumber == roomDTO.RoomNumber && r.BuildingId == roomDTO.BuildingId);

            if (existingRoom != null)
            {
                return BadRequest("RoomNumber đã tồn tại trong Building này.");
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

        [HttpGet("CheckRoomNameExists")]
        public IActionResult CheckRoomNameExists(string roomName, int buildingId)
        {
            var roomExists = _context.Rooms.Any(r => r.RoomNumber == roomName && r.BuildingId == buildingId);
            return Ok(roomExists); // Trả về true nếu phòng tồn tại, ngược lại trả về false
        }


        [HttpPut("UpdateRoom")]
        public IActionResult UpdateRoom([FromBody] RoomLlUpdateDTO roomDto)
        {
            if (roomDto == null)
            {
                return BadRequest("Sai dữ liệu");
            }

            // Kiểm tra xem phòng với Id có tồn tại trong cơ sở dữ liệu không
            var room = _context.Rooms.FirstOrDefault(r => r.Id == roomDto.Id);

            if (room == null)
            {
                return NotFound("Không tìm thấy phòng có Id = " + roomDto.Id);
            }

            // Lấy BuildingId từ phòng hiện tại
            var buildingId = room.BuildingId;

            // Kiểm tra xem RoomNumber có bị trùng trong cùng tòa nhà không
            var roomExists = _context.Rooms
                .Any(r => r.BuildingId == buildingId && r.RoomNumber == roomDto.RoomNumber && r.Id != roomDto.Id);

            if (roomExists)
            {
                return BadRequest("Số phòng đã tồn tại.");
            }

            // Cập nhật thông tin phòng
            room.Price = roomDto.Price;
            room.RoomNumber = roomDto.RoomNumber;
            room.Area = roomDto.Area;
            room.Description = roomDto.Description;
            room.Floor = roomDto.Floor;
            room.StartedDate = roomDto.StartedDate;
            room.ExpiredDate = roomDto.ExpiredDate;
            room.RoomStatusId = roomDto.RoomStatusId;
            room.FreeInFutureDate = roomDto.FreeInFutureDate;

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            return Ok("Cập nhật phòng thành công");
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

            var images = _context.Images.Where(i => i.RoomId == roomId);
            _context.Images.RemoveRange(images);

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

                var images = _context.Images.Where(i => i.RoomId == room.Id);
                _context.Images.RemoveRange(images);
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
                .Where(r => r.RoomStatusId == 1 & r.Building.BuildingStatusId == 1) // Lọc các phòng có trạng thái đang hoạt động
                .Select(r => new
                {
                    Id = r.Id,
                    Distance = r.Building.Distance,
                    Address = $"{r.Building.Address.Information}, {r.Building.Address.Ward.Name}, {r.Building.Address.District.Name}, {r.Building.Address.Province.Name}",
                    Price = r.Price,
                    Area = r.Area,
                    RoomStatusName = r.RoomStatus.Name,
                    //images = r.images.select(i => i.link).tolist()
                })
                .ToList();

            return Ok(rooms);
        }

        [HttpGet("detail/{id}")]
        public IActionResult GetRoomDetail(int id)
        {
            // Lấy thông tin phòng theo ID
            var room = _context.Rooms
                .Where(r => r.RoomStatusId == 1 || r.RoomStatusId == 4 & r.Building.BuildingStatusId == 1)
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
                            (r.RoomStatusId == 1) &&
                            r.Id != roomId) // Loại trừ phòng hiện tại
                .OrderBy(r => r.Price) // Sắp xếp theo giá tiền, nếu cần
                .Take(5) // Lấy top 5 phòng
                .Select(r => new SuggestedRoomDTO
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    Price = r.Price,
                    Area = r.Area,
                    RoomStatusName = r.RoomStatus.Name,
                    //Images = r.Images.Select(i => i.Url).ToList() // Giả sử có liên kết tới bảng Images
                })
                .ToList();

            return Ok(suggestedRooms);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchRooms([FromQuery] RoomSearchDTO searchDto)
        {
            var rooms = await _context.Rooms
                .Where(r => r.Building.BuildingStatusId == 1)
                .Include(r => r.Building)
                .ThenInclude(b => b.Province)
                .Include(r => r.Building.District)
                .Include(r => r.Building.Ward)
                .Include(r => r.RoomStatus)
                .Where(r =>
                    (searchDto.ProvinceId == 0 || r.Building.ProvinceId == searchDto.ProvinceId) &&
                    (searchDto.DistrictId == 0 || r.Building.DistrictId == searchDto.DistrictId) &&
                    (searchDto.WardId == 0 || r.Building.WardId == searchDto.WardId) &&
                    (string.IsNullOrEmpty(searchDto.RoomStatus) || r.RoomStatus.Name == searchDto.RoomStatus) &&
                    (searchDto.MinDistance == null || r.Building.Distance >= searchDto.MinDistance) &&
                    (searchDto.MaxDistance == null || r.Building.Distance <= searchDto.MaxDistance) &&
                    (searchDto.MinPrice == null || r.Price >= searchDto.MinPrice) &&
                    (searchDto.MaxPrice == null || r.Price <= searchDto.MaxPrice) &&
                    (searchDto.MinArea == null || r.Area >= searchDto.MinArea) &&
                    (searchDto.MaxArea == null || r.Area <= searchDto.MaxArea)
                )
                .Select(r => new
                {
                    Id = r.Id,
                    Building = r.Building.Name,
                    Distance = r.Building.Distance,
                    Address = $"{r.Building.Address.Information}, {r.Building.Address.Ward.Name}, {r.Building.Address.District.Name}, {r.Building.Address.Province.Name}",
                    Price = r.Price,
                    Area = r.Area,
                    RoomStatusName = r.RoomStatus.Name,
                    //Images = r.Images.Select(i => i.Link).ToList()
                })
                .ToListAsync();

            if (!rooms.Any())
            {
                return NotFound("Không tìm thấy phòng nào phù hợp.");
            }

            return Ok(rooms);
        }

        [HttpGet("qr/{roomId}")]
        public IActionResult GetRoomQrCode(int roomId)
        {
            string baseUrl = $"https://localhost:5001";
            string qrContent = $"{baseUrl}/Room/RoomMaintainance/{roomId}";

            // Tạo mã QR
            var writer = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = 300, // Chiều cao hình ảnh
                    Width = 300,  // Chiều rộng hình ảnh
                    Margin = 1    // Lề
                }
            };

            var pixelData = writer.Write(qrContent);

            // Chuyển dữ liệu pixel thành ảnh PNG
            using var ms = new MemoryStream();
            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                var bitmapData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    bitmap.PixelFormat);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            }

            // Trả về hình ảnh dưới dạng response
            return File(ms.ToArray(), "image/png");
        }
        [HttpGet("RoomMaintainance/{roomId}")]
        public IActionResult GetByQR(int roomId)
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
                .Include(r => r.Building)
                    .ThenInclude(b => b.User)
                .FirstOrDefault(r => r.Id == roomId);

            string address = room.Building.Address.Information + " " + room.Building.Ward.Name + " " + room.Building.District.Name + " " + room.Building.Province.Name;

            var qrData = new
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                BuildingName = room.Building.Name,
                Address = address,
                Owner = room.Building.User.FirstName,
            };

            return Ok(qrData);
        }

        [HttpPost("SaveMaintenanceRequest")]
        public IActionResult SaveMaintainancebyQR([FromBody] MaintainanceDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid maintenance request data.");

            var maintainance = new MaintainanceRequest
            {
                Description = dto.Description,
                Status = dto.Status,
                RoomId = dto.RoomId,
            };

            _context.MaintainanceRequests.Add(maintainance);
            _context.SaveChanges();

            return Ok("Maintenance request saved successfully.");
        }
    }
}
