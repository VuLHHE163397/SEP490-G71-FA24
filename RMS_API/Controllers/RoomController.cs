﻿using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("GetAllRoom/{userId}")]
        public IActionResult GetAllRoomByUserId(int userId)
        {
            var ro = _context.Rooms
                .Select(e => new Room
                {
                    Id = e.Id,
                    RoomNumber = e.RoomNumber,
                    BuildingId = e.BuildingId,
                    Description = e.Description,
                    ExpiredDate = e.ExpiredDate,
                    Building = _context.Buildings.FirstOrDefault(x => x.Id == e.BuildingId),
                })
                .Where(e => e.Building.UserId == userId)
                .ToList();
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

        [HttpGet("GetServiceByBuilding/{buildingId}")]
        public IActionResult GetServiceByBuilding(int buildingId)
        {
            var service = _context.Services.Where(p => p.BuildingId == buildingId).ToList();
            return Ok(service);
        }

        [HttpGet("GetServiceByRoom/{roomId}")]
        public IActionResult GetServiceByRoom(int roomId)
        {
            try
            {
                // Lấy danh sách dịch vụ liên quan đến buildingId
                var services = _context.Services

                    .Where(s => s.Rooms.Any(r => r.Id == roomId))
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.Price
                    })
                    .ToList();

                if (services == null || !services.Any())
                {
                    return NotFound(new { message = "Không tìm thây dịch vụ của phòng !!!" });
                }
                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the services.", details = ex.Message });
            }
        }

        [HttpPost("UpdateServicesForRoom/{roomId}")]
        public async Task<IActionResult> UpdateServicesForRoom(int roomId, [FromBody] List<int> serviceIds)
        {
            try
            {
                // Kiểm tra danh sách serviceIds
                //if (serviceIds == null || !serviceIds.Any())
                //{
                //    return BadRequest(new { message = "Danh sách dịch vụ không hợp lệ." });
                //}

                var room = await _context.Rooms.Include(r => r.Services).FirstOrDefaultAsync(r => r.Id == roomId);
                if (room == null)
                {
                    return NotFound(new { message = "Phòng không tồn tại." });
                }

                if (serviceIds == null || !serviceIds.Any())
                {
                    room.Services.Clear();
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Đã xóa toàn bộ dịch vụ của phòng." });
                }

                // Log danh sách dịch vụ hiện tại của phòng
                Console.WriteLine("Dịch vụ hiện tại: " + string.Join(", ", room.Services.Select(s => s.Id)));

                // Xóa các dịch vụ không có trong danh sách đã gửi
                var servicesToRemove = room.Services.Where(s => !serviceIds.Contains(s.Id)).ToList();

                foreach (var service in servicesToRemove)
                {
                    room.Services.Remove(service);
                }

                // Thêm các dịch vụ mới vào phòng
                foreach (var serviceId in serviceIds)
                {
                    var service = await _context.Services.FindAsync(serviceId);
                    if (service != null && !room.Services.Contains(service))
                    {
                        room.Services.Add(service);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Dịch vụ đã được cập nhật thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi cập nhật dịch vụ.", details = ex.Message });
            }
        }


        [HttpGet("GetServicesForRoom/{roomId}")]
        public async Task<IActionResult> GetServicesForRoom(int roomId)
        {
            try
            {
                // Lấy danh sách tất cả dịch vụ của tòa nhà
                var allServices = await _context.Services.ToListAsync();

                // Lấy dịch vụ đã được gán cho phòng này
                var roomServices = await _context.Rooms
                    .Where(r => r.Id == roomId)
                    .Include(r => r.Services)
                    .FirstOrDefaultAsync();

                if (roomServices == null)
                {
                    return NotFound(new { message = "Room not found." });
                }

                // Lấy danh sách các dịch vụ của phòng dưới dạng object với trạng thái checked
                var services = allServices.Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Price,
                    IsChecked = roomServices.Services.Any(rs => rs.Id == s.Id)
                }).ToList();

                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }


        // Tạo một dictionary ánh xạ trạng thái phòng sang RoomStatusId
        private static readonly Dictionary<string, int> RoomStatusMapping = new Dictionary<string, int>
        {
            { "Đang trống", 1 },
            { "Đang cho thuê", 2 },
            { "Đang bảo trì", 3 },
            { "Sắp trống", 4 }
        };

        //[HttpPost("ImportRooms/{buildingId}/{userId}")]
        //public async Task<IActionResult> ImportRooms(IFormFile file, int buildingId, int userId)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    // Lấy thông tin tòa nhà để kiểm tra giới hạn số phòng
        //    var building = _context.Buildings.FirstOrDefault(b => b.Id == buildingId);
        //    if (building == null)
        //    {
        //        return NotFound("Tòa nhà không tồn tại.");
        //    }

        //    int maxRoomsPerBuilding = (int)building.NumberOfRooms; // Số lượng phòng tối đa từ Building
        //    int existingRoomCount = _context.Rooms.Count(r => r.BuildingId == buildingId); // Số phòng hiện có
        //    int remainingRoomSpace = maxRoomsPerBuilding - existingRoomCount; // Số phòng còn trống

        //    if (file.FileName.EndsWith(".xlsx"))
        //    {
        //        var roomsToAdd = new List<Room>();
        //        var existingRoomsToUpdate = new List<Room>();

        //        using (var stream = new MemoryStream())
        //        {
        //            await file.CopyToAsync(stream);
        //            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //            using (var package = new ExcelPackage(stream))
        //            {
        //                var worksheet = package.Workbook.Worksheets[0];
        //                var rowCount = worksheet.Dimension.Rows;
        //                var colCount = worksheet.Dimension.Columns;

        //                var headers = new Dictionary<string, int>();
        //                for (int col = 1; col <= colCount; col++)
        //                {
        //                    headers[worksheet.Cells[1, col].Text.Trim()] = col;
        //                }

        //                // Kiểm tra định dạng file Excel
        //                if (!headers.ContainsKey("Số phòng") ||
        //                    !headers.ContainsKey("Diện tích") ||
        //                    !headers.ContainsKey("Tầng") ||
        //                    !headers.ContainsKey("Giá phòng") ||
        //                    !headers.ContainsKey("Trạng thái"))
        //                {
        //                    return BadRequest("Tên cột trong file excel bị sai. Vui lòng kiểm tra lại.");
        //                }

        //                // Lấy danh sách tất cả các phòng hiện có trong database cho tòa nhà
        //                var existingRoomNumbers = _context.Rooms
        //                    .Where(r => r.BuildingId == buildingId)
        //                    .ToDictionary(r => r.RoomNumber, r => r);

        //                // Danh sách các số phòng được import
        //                var importedRoomNumbers = new HashSet<string>();
        //                int newRoomsCount = 0;

        //                for (int row = 2; row <= rowCount; row++)
        //                {
        //                    var roomNumber = worksheet.Cells[row, headers["Số phòng"]].Text.Trim();
        //                    if (string.IsNullOrEmpty(roomNumber)) continue;

        //                    importedRoomNumbers.Add(roomNumber); // Thêm vào danh sách được import

        //                    // Validate dữ liệu từng cột
        //                    if (!double.TryParse(worksheet.Cells[row, headers["Diện tích"]].Text, out var area) || area <= 0)
        //                    {
        //                        return BadRequest($"Giá trị lỗi ở hàng {row}: '{worksheet.Cells[row, headers["Diện tích"]].Text}'. Vui lòng nhập diện tích là số dương.");
        //                    }
        //                    if (!int.TryParse(worksheet.Cells[row, headers["Tầng"]].Text, out var floor) || floor <= 0)
        //                    {
        //                        return BadRequest($"Giá trị lỗi ở hàng {row}: '{worksheet.Cells[row, headers["Tầng"]].Text}'. Vui lòng nhập tầng là số nguyên dương.");
        //                    }
        //                    var rawPrice = worksheet.Cells[row, headers["Giá phòng"]].Text.Trim()
        //                        .Replace("VNĐ", "").Replace(",", "").Replace(" ", "");
        //                    if (!decimal.TryParse(rawPrice, out var price) || price <= 0)
        //                    {
        //                        return BadRequest($"Giá trị lỗi ở hàng {row}: '{worksheet.Cells[row, headers["Giá phòng"]].Text}'. Vui lòng nhập giá phòng là số nguyên dương.");
        //                    }
        //                    var status = worksheet.Cells[row, headers["Trạng thái"]].Text.Trim();
        //                    var description = headers.ContainsKey("Mô tả phòng")
        //                        ? worksheet.Cells[row, headers["Mô tả phòng"]].Text.Trim()
        //                        : null;

        //                    var roomStatusId = RoomStatusMapping.ContainsKey(status) ? RoomStatusMapping[status] : (int?)null;
        //                    if (roomStatusId == null)
        //                    {
        //                        return BadRequest($"Giá trị lỗi ở hàng {row}: '{status}'. Vui lòng nhập trạng thái hợp lệ: Đang Trống, Đang cho thuê, Đang bảo trì, Sắp trống.");
        //                    }

        //                    if (existingRoomNumbers.ContainsKey(roomNumber))
        //                    {
        //                        // Cập nhật thông tin phòng đã tồn tại
        //                        var existingRoom = existingRoomNumbers[roomNumber];
        //                        existingRoom.Price = price;
        //                        existingRoom.Area = area;
        //                        existingRoom.Description = description;
        //                        existingRoom.Floor = floor;
        //                        existingRoom.RoomStatusId = roomStatusId.Value;
        //                        existingRoomsToUpdate.Add(existingRoom);
        //                    }
        //                    else
        //                    {
        //                        // Chỉ kiểm tra giới hạn khi thêm phòng mới
        //                        if (newRoomsCount < remainingRoomSpace)
        //                        {
        //                            var newRoom = new Room
        //                            {
        //                                BuildingId = buildingId,
        //                                RoomNumber = roomNumber,
        //                                Price = price,
        //                                Area = area,
        //                                Description = description,
        //                                Floor = floor,
        //                                RoomStatusId = roomStatusId.Value,
        //                                UserId = userId
        //                            };
        //                            roomsToAdd.Add(newRoom);
        //                            newRoomsCount++;
        //                        }
        //                        else
        //                        {
        //                            return Ok(new
        //                            {
        //                                message = $"Tòa nhà đã đạt giới hạn số phòng tối đa là ({maxRoomsPerBuilding} phòng). Kiểm tra lại file và xóa các phòng thừa.",
        //                                newRoomsCount = roomsToAdd.Count,
        //                                updatedRoomsCount = existingRoomsToUpdate.Count,
        //                                buildingFull = true // Thêm flag để báo hiệu tòa nhà đầy
        //                            });
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        // Thêm mới và cập nhật dữ liệu vào database
        //        if (roomsToAdd.Any())
        //        {
        //            _context.Rooms.AddRange(roomsToAdd);
        //        }

        //        if (existingRoomsToUpdate.Any())
        //        {
        //            _context.Rooms.UpdateRange(existingRoomsToUpdate);
        //        }

        //        await _context.SaveChangesAsync();

        //        return Ok(new
        //        {
        //            message = $"Nhập phòng thành công! Thêm mới {roomsToAdd.Count} phòng, cập nhật {existingRoomsToUpdate.Count} phòng.",
        //            newRoomsCount = roomsToAdd.Count,
        //            updatedRoomsCount = existingRoomsToUpdate.Count
        //        });
        //    }
        //    else
        //    {
        //        return BadRequest("Only Excel files are allowed.");
        //    }
        //}


        [HttpPost("ImportRooms/{buildingId}/{userId}")]
        public async Task<IActionResult> ImportRooms(IFormFile file, int buildingId, int userId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var building = _context.Buildings.FirstOrDefault(b => b.Id == buildingId);
            if (building == null)
            {
                return NotFound("Tòa nhà không tồn tại.");
            }

            int maxRoomsPerBuilding = (int)building.NumberOfRooms;
            int existingRoomCount = _context.Rooms.Count(r => r.BuildingId == buildingId);
            int remainingRoomSpace = maxRoomsPerBuilding - existingRoomCount;

            if (file.FileName.EndsWith(".xlsx"))
            {
                var roomsToAdd = new List<Room>();
                var existingRoomsToUpdate = new List<Room>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;
                        var colCount = worksheet.Dimension.Columns;

                        var headers = new Dictionary<string, int>();
                        for (int col = 1; col <= colCount; col++)
                        {
                            headers[worksheet.Cells[1, col].Text.Trim()] = col;
                        }

                        if (!headers.ContainsKey("Số phòng") ||
                            !headers.ContainsKey("Diện tích") ||
                            !headers.ContainsKey("Tầng") ||
                            !headers.ContainsKey("Giá phòng") ||
                            !headers.ContainsKey("Trạng thái"))
                        {
                            return BadRequest("Tên cột trong file excel bị sai. Vui lòng kiểm tra lại.");
                        }

                        var existingRoomNumbers = _context.Rooms
                            .Where(r => r.BuildingId == buildingId)
                            .ToDictionary(r => r.RoomNumber, r => r);

                        var lastRoomRows = new Dictionary<string, int>();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var roomNumber = worksheet.Cells[row, headers["Số phòng"]].Text.Trim();
                            if (!string.IsNullOrEmpty(roomNumber))
                            {
                                lastRoomRows[roomNumber] = row; // Ghi đè, chỉ giữ lại hàng cuối cùng
                            }
                        }

                        int newRoomsCount = 0;
                        foreach (var kvp in lastRoomRows)
                        {
                            var roomNumber = kvp.Key;
                            var row = kvp.Value;

                            if (!double.TryParse(worksheet.Cells[row, headers["Diện tích"]].Text, out var area) || area <= 0)
                            {
                                return BadRequest($"Giá trị lỗi ở hàng {row}: '{worksheet.Cells[row, headers["Diện tích"]].Text}'. Vui lòng nhập diện tích là số dương.");
                            }
                            if (!int.TryParse(worksheet.Cells[row, headers["Tầng"]].Text, out var floor) || floor <= 0)
                            {
                                return BadRequest($"Giá trị lỗi ở hàng {row}: '{worksheet.Cells[row, headers["Tầng"]].Text}'. Vui lòng nhập tầng là số nguyên dương.");
                            }
                            var rawPrice = worksheet.Cells[row, headers["Giá phòng"]].Text.Trim()
                                .Replace("VNĐ", "").Replace(",", "").Replace(" ", "");
                            if (!decimal.TryParse(rawPrice, out var price) || price <= 0)
                            {
                                return BadRequest($"Giá trị lỗi ở hàng {row}: '{worksheet.Cells[row, headers["Giá phòng"]].Text}'. Vui lòng nhập giá phòng là số nguyên dương.");
                            }
                            var status = worksheet.Cells[row, headers["Trạng thái"]].Text.Trim();
                            var description = headers.ContainsKey("Mô tả phòng")
                                ? worksheet.Cells[row, headers["Mô tả phòng"]].Text.Trim()
                                : null;

                            var roomStatusId = RoomStatusMapping.ContainsKey(status) ? RoomStatusMapping[status] : (int?)null;
                            if (roomStatusId == null)
                            {
                                return BadRequest($"Giá trị lỗi ở hàng {row}: '{status}'. Vui lòng nhập trạng thái hợp lệ: Đang Trống, Đang cho thuê, Đang bảo trì, Sắp trống.");
                            }

                            if (existingRoomNumbers.ContainsKey(roomNumber))
                            {
                                var existingRoom = existingRoomNumbers[roomNumber];
                                existingRoom.Price = price;
                                existingRoom.Area = area;
                                existingRoom.Description = description;
                                existingRoom.Floor = floor;
                                existingRoom.RoomStatusId = roomStatusId.Value;
                                existingRoomsToUpdate.Add(existingRoom);
                            }
                            else
                            {
                                if (newRoomsCount < remainingRoomSpace)
                                {
                                    var newRoom = new Room
                                    {
                                        BuildingId = buildingId,
                                        RoomNumber = roomNumber,
                                        Price = price,
                                        Area = area,
                                        Description = description,
                                        Floor = floor,
                                        RoomStatusId = roomStatusId.Value,
                                        UserId = userId
                                    };
                                    roomsToAdd.Add(newRoom);
                                    newRoomsCount++;
                                }
                                else
                                {
                                    return Ok(new
                                    {
                                        message = $"Tòa nhà đã đạt giới hạn số phòng tối đa là ({maxRoomsPerBuilding} phòng). Kiểm tra lại file và xóa các phòng thừa.",
                                        newRoomsCount = roomsToAdd.Count,
                                        updatedRoomsCount = existingRoomsToUpdate.Count,
                                        buildingFull = true
                                    });
                                }
                            }
                        }
                    }
                }

                if (roomsToAdd.Any())
                {
                    _context.Rooms.AddRange(roomsToAdd);
                }

                if (existingRoomsToUpdate.Any())
                {
                    _context.Rooms.UpdateRange(existingRoomsToUpdate);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Nhập phòng thành công! Thêm mới {roomsToAdd.Count} phòng, cập nhật {existingRoomsToUpdate.Count} phòng.",
                    newRoomsCount = roomsToAdd.Count,
                    updatedRoomsCount = existingRoomsToUpdate.Count
                });
            }
            else
            {
                return BadRequest("Only Excel files are allowed.");
            }
        }


        [HttpPost("upload-image/{roomId}")]
        public async Task<IActionResult> UploadImage(IFormFile file, int roomId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Upload ảnh lên Cloudinary
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = "room_images"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return StatusCode(500, "Failed to upload image.");
            }

            // Lưu link ảnh vào database
            var image = new Models.Image
            {
                Link = uploadResult.SecureUrl.AbsoluteUri,
                RoomId = roomId
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return Ok(new { imageId = image.Id, imageUrl = image.Link });
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
            // Kiểm tra xem tòa nhà có tồn tại hay không
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == roomDTO.BuildingId);
            if (building == null)
            {
                return BadRequest("Tòa nhà không tồn tại.");
            }

            // Kiểm tra số lượng phòng đã đạt giới hạn chưa
            var currentRoomCount = await _context.Rooms.CountAsync(r => r.BuildingId == roomDTO.BuildingId);
            if (building.NumberOfRooms.HasValue && currentRoomCount >= building.NumberOfRooms)
            {
                return BadRequest("Số lượng phòng trong tòa nhà đã đạt giới hạn.");
            }

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
                UserId = roomDTO.UserId,
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return Ok(room);
        }


        [HttpGet("CheckRoomNameExists")]
        public async Task<IActionResult> CheckRoomNameExists(string roomName, int buildingId)
        {
            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomNumber == roomName && r.BuildingId == buildingId);
            return Ok(roomExists); // Trả về true nếu phòng tồn tại, ngược lại trả về false
        }

        [HttpGet("GetTotalFloorByBuildingId/{buildingId}")]
        public async Task<IActionResult> GetNumberOfRoomsByBuildingId(int buildingId)
        {
            var num = await _context.Buildings.FirstOrDefaultAsync(p => p.Id == buildingId);
            return Ok(num.TotalFloors);
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

        [HttpPut("updateRoomStatus/{roomId}")]
        public IActionResult UpdateRoomStatus(int roomId, [FromBody] int statusId)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null)
            {
                return NotFound("Không tìm thấy phòng có id = " + roomId);
            }

            room.RoomStatusId = statusId;
            _context.SaveChanges();
            return Ok("Cập nhật trạng thái phòng thành công!!!");
        }


        [HttpGet("GetActiveRooms")]
        public IActionResult GetActiveRooms()
        {
            var rooms = _context.Rooms
                .Where(r => r.RoomStatusId == 1 && r.Building.BuildingStatusId == 1)
                .Select(r => new RoomDTO
                {
                    Id = r.Id,
                    Building = r.Building.Name,
                    RoomNumber = r.RoomNumber,
                    Address = $"{r.Building.Address.Information}, {r.Building.Address.Ward.Name}, {r.Building.Address.District.Name}, {r.Building.Address.Province.Name}",
                    Price = r.Price,
                    Area = r.Area,
                    RoomStatusName = r.RoomStatus.Name,
                    Distance = r.Building.Distance,
                    FirstImageLink = r.Images.OrderBy(i => i.Id).Select(i => i.Link).FirstOrDefault() // Lấy link ảnh đầu tiên
                })
                .OrderByDescending(r => r.Id) // Sắp xếp theo Id giảm dần
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
                Building = room.Building.Name,
                RoomNumber = room.RoomNumber,
                Facebook = room.Building?.User.FacebookUrl,
                Zalo = room.Building?.User.ZaloUrl ?? "Chưa có thông tin Zalo",
                FullAddress = $"{room.Building?.Address?.Information ?? "Chưa có địa chỉ chi tiết"}, " +
                               $"{room.Building?.Ward?.Name ?? "Chưa có phường"}, " +
                               $"{room.Building?.District?.Name ?? "Chưa có quận"}, " +
                               $"{room.Building?.Province?.Name ?? "Chưa có tỉnh"}",
                Price = room.Price,
                Area = room.Area,
                Distance = room.Building?.Distance ?? 0,
                FreeInFutureDate = room.FreeInFutureDate,
                Description = room.Description,
                RoomStatus = room.RoomStatus?.Name ?? "Trạng thái không xác định",
                OwnerName = $"{room.Building?.User?.LastName ?? ""} " +
                            $"{room.Building?.User?.MidName ?? ""} " +
                            $"{room.Building?.User?.FirstName ?? ""}".Trim(),
                OwnerPhone = room.Building?.User?.Phone ?? "Số điện thoại không có sẵn",
                LinkEmbedMap = room.Building?.LinkEmbedMap ?? "Không có liên kết bản đồ",
                Images = room.Images.Select(img => new ImageDTO
                {
                    Link = img.Link,  // Lấy đường dẫn ảnh từ bảng Images
                    RoomId = img.RoomId
                }).ToList()
            };

            return Ok(roomDetailDto); // Trả về dữ liệu chi tiết phòng cùng ảnh
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
                    FirstImageLink = r.Images.OrderBy(i => i.Id).Select(i => i.Link).FirstOrDefault()
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
                .OrderBy(r => r.FreeInFutureDate.HasValue ? r.FreeInFutureDate.Value : DateTime.MaxValue) // Sắp xếp theo FreeInFutureDate
                .Select(r => new
                {
                    Id = r.Id,
                    Building = r.Building.Name,
                    RoomNumber = r.RoomNumber,
                    Distance = r.Building.Distance,
                    Address = $"{r.Building.Address.Information}, {r.Building.Address.Ward.Name}, {r.Building.Address.District.Name}, {r.Building.Address.Province.Name}",
                    Price = r.Price,
                    Area = r.Area,
                    RoomStatusName = r.RoomStatus.Name,
                    FreeInFutureDate = r.FreeInFutureDate.HasValue ? r.FreeInFutureDate.Value.ToString("dd/MM/yyyy") : "",
                    FirstImageLink = r.Images.OrderBy(i => i.Id).Select(i => i.Link).FirstOrDefault()
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
                Owner = $"{room.Building?.User?.LastName ?? ""} " +
                            $"{room.Building?.User?.MidName ?? ""} " +
                            $"{room.Building?.User?.FirstName ?? ""}".Trim(),
            };

            return Ok(qrData);
        }

        [HttpPost("SaveMaintenanceRequest")]

        public IActionResult SaveMaintainancebyQR([FromBody] MaintainanceDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { Success = false, Message = "Nội dung báo cáo không được để trống." });

            var maintainance = new MaintainanceRequest
            {
                Description = dto.Description,
                Status = dto.Status,
                RoomId = dto.RoomId,
                RequestDate = dto.RequestDate
            };

            _context.MaintainanceRequests.Add(maintainance);
            _context.SaveChanges();

            return Ok(new { Success = true, Message = "Gửi báo cáo thành công!" });
        }
    }
}