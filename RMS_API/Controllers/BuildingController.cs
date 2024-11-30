using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RMS_API.DTOs;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingController : Controller
    {
        private readonly RMS_SEP490Context _context;

        public BuildingController(RMS_SEP490Context context)
        {
            _context = context;
        }

        

        [HttpGet("GetBuildingsByUserId/{userId}")]
        [Authorize(Roles = "Landlord")]         // Chỉ cho phép Landlord truy cập
        public async Task<IActionResult> GetBuildingsByUserId(int userId)
        {
            // Lấy các tòa nhà thuộc về userId từ database
            var buildings = await _context.Buildings
                .Include(b => b.Address)
                .Include(b => b.BuildingStatus)
                .Include(b => b.Province)
                .Include(b => b.District)
                .Include(b => b.Ward)
                .Where(b => b.UserId == userId) // Lọc theo UserId
                .Select(b => new BuildingDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    TotalFloors = b.TotalFloors,
                    NumberOfRooms = (int)b.NumberOfRooms,
                    Distance = b.Distance,
                    LinkEmbedMap = b.LinkEmbedMap,
                    CreatedDate = b.CreatedDate,
                    UpdatedDate = b.UpdatedDate,
                    ProvinceName = b.Province.Name,
                    DistrictName = b.District.Name,
                    WardName = b.Ward.Name,
                    AddressDetails = $"{b.Address.Information},{b.Address.Ward.Name}, {b.Address.District.Name}, {b.Address.Province.Name}",
                    BuildingStatus = b.BuildingStatus.Name
                })
                .ToListAsync();

            // Kiểm tra nếu không có tòa nhà nào
            if (!buildings.Any())
            {
                return NotFound($"No buildings found for user with ID {userId}.");
            }

            // Trả về danh sách các tòa nhà dưới dạng JSON
            return Ok(buildings);
        }



        [HttpGet("GetAllBuildings")]
        public IActionResult GetAllBuildings()
        {
            var buildings = _context.Buildings
                .Include(b => b.Address)
                .Include(b => b.BuildingStatus)
                .Include(b => b.User)
                .Include(b => b.Province)
                .Include(b => b.District)
                .Include(b => b.Ward)
                .Select(b => new BuildingDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    TotalFloors = b.TotalFloors,
                    NumberOfRooms = (int)b.NumberOfRooms,
                    Distance = b.Distance,
                    LinkEmbedMap = b.LinkEmbedMap,
                    CreatedDate = b.CreatedDate,
                    UpdatedDate = b.UpdatedDate,
                    ProvinceName = b.Province.Name,
                    DistrictName = b.District.Name,
                    WardName = b.Ward.Name,
                    AddressDetails = $"{b.Address.Information},{b.Address.Ward.Name}, {b.Address.District.Name}, {b.Address.Province.Name}",
                    BuildingStatus = b.BuildingStatus.Name
                })
                .ToList();

            return Ok(buildings);
        }

        [HttpGet("GetProvinces")]
        public IActionResult GetProvinces()
        {
            var provinces = _context.Provinces
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.CreatedDate,
                    p.LastModifiedDate
                })
                .ToList();

            if (!provinces.Any())
            {
                return NotFound("No provinces found.");
            }

            return Ok(provinces);
        }

        [HttpGet("GetDistrictsByProvince/{provinceName}")]
        public IActionResult GetDistrictsByProvince(string provinceName)
        {
            if (string.IsNullOrEmpty(provinceName))
            {
                return BadRequest("Province name cannot be null or empty.");
            }

            var districts = _context.Districts
                .Where(d => d.Provinces.Name == provinceName)
                .Select(d => new
                {
                    Id = d.Id,
                    Name = d.Name
                }).ToList();

            if (!districts.Any())
            {
                return NotFound("No districts found for the given province.");
            }

            return Ok(districts); // Trả về dưới dạng JSON
        }

        [HttpGet("GetWardsByDistrict/{districtName}")]
        public IActionResult GetWardsByDistrict(string districtName)
        {
            var wards = _context.Wards
                .Where(w => w.District.Name == districtName)
                .Select(w => new
                {
                    Id = w.Id,
                    Name = w.Name
                }).ToList();

            return Ok(wards);
        }


        [HttpGet("CheckBuildingName/{userId}/{name}")]
        public IActionResult CheckBuildingName(int userId, string name)
        {
            if (string.IsNullOrEmpty(name) || userId <= 0)
            {
                return BadRequest(new { message = "Invalid input data." });
            }

            // Kiểm tra tên tòa nhà đã tồn tại cho UserId hiện tại
            var isDuplicateForCurrentUser = _context.Buildings.Any(b => b.UserId == userId && b.Name == name);

            if (isDuplicateForCurrentUser)
            {
                return Conflict(new { message = "Building name already exists for the specified user." });
            }

            // Nếu tên tòa nhà trùng với một userId khác, cho phép tạo mới
            return Ok(new { message = "Building name is available for the specified user or different users." });
        }

        [HttpPost("AddBuildingbyId")]
        public async Task<IActionResult> AddBuildingbyId([FromBody] AddBuildingDTO buildingDto)
        {
            // Kiểm tra tên tòa nhà đã tồn tại cho UserId hiện tại
            var isDuplicateForCurrentUser = await _context.Buildings
                .AnyAsync(b => b.UserId == buildingDto.UserId && b.Name == buildingDto.Name);

            if (isDuplicateForCurrentUser)
            {
                return Conflict("Tên tòa nhà trùng với tên hiện có. Vui lòng nhập lại.");
            }

            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra UserId có tồn tại không
            var userExists = await _context.Users.AnyAsync(u => u.Id == buildingDto.UserId);
            if (!userExists)
            {
                return BadRequest($"User with ID {buildingDto.UserId} does not exist.");
            }

            // Tìm Province
            var province = await _context.Provinces
                .FirstOrDefaultAsync(p => p.Name.Equals(buildingDto.ProvinceName));

            if (province == null)
            {
                return BadRequest($"Province '{buildingDto.ProvinceName}' not found.");
            }

            // Tìm District
            var district = await _context.Districts
                .FirstOrDefaultAsync(d => d.Name.Equals(buildingDto.DistrictName) && d.ProvincesId == province.Id);

            if (district == null)
            {
                return BadRequest($"District '{buildingDto.DistrictName}' not found in province '{buildingDto.ProvinceName}'.");
            }

            // Tìm Ward
            var ward = await _context.Wards
                .FirstOrDefaultAsync(w => w.Name.Equals(buildingDto.WardName) && w.DistrictId == district.Id);

            if (ward == null)
            {
                return BadRequest($"Ward '{buildingDto.WardName}' not found in district '{buildingDto.DistrictName}'.");
            }

            // Tìm BuildingStatus
            var buildingStatus = await _context.BuildingStatuses
                .FirstOrDefaultAsync(bs => bs.Name.Equals(buildingDto.BuildingStatus));

            if (buildingStatus == null)
            {
                return BadRequest($"Building status '{buildingDto.BuildingStatus}' not found.");
            }

            // Tạo mới Address
            var address = new Address
            {
                Information = buildingDto.AddressDetails,
                DistrictId = district.Id,
                WardId = ward.Id,
                ProvinceId = province.Id
            };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            // Tạo mới Building
            var building = new Building
            {
                Name = buildingDto.Name,
                TotalFloors = buildingDto.TotalFloors,
                NumberOfRooms = buildingDto.NumberOfRooms,
                Distance = buildingDto.Distance,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                ProvinceId = province.Id,
                DistrictId = district.Id,
                WardId = ward.Id,
                AddressId = address.Id,
                BuildingStatusId = buildingStatus.Id,
                UserId = buildingDto.UserId,
                LinkEmbedMap = buildingDto.LinkEmbedMap
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            // Trả về thông tin tòa nhà đã được tạo
            var responseDto = new
            {
                building.Name,
                building.TotalFloors,
                building.NumberOfRooms,
                building.Distance,
                building.AddressId,
                building.UserId,
                building.BuildingStatusId
            };

            return Ok(responseDto);
        }






      
        [HttpDelete("DeleteBuilding/{id}")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == id);

            if (building == null)
            {
                return NotFound("Building not found.");
            }


            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();

            return Ok("Building deleted successfully.");
        }


        /*[HttpGet("EditBuilding")]
        public IActionResult EditBuilding()
        {
            ViewBag.Provinces = _context.Provinces.ToList();

            ViewBag.BuildingStatuses = _context.BuildingStatuses.ToList();

            return View();
        }*/



        [HttpGet("GetBuildinImformationgById/{id}")]
        public async Task<IActionResult> GetBuildinImformationgById(int id)
        {
            var building = await _context.Buildings
               .Include(b => b.Address)
               .Include(b => b.BuildingStatus)
               .Include(b => b.User)
               .Include(b => b.Province)
               .Include(b => b.District)
               .Include(b => b.Ward)
               .Where(b => b.Id == id)
               .Select(b => new BuildingDTO
               {
                   Id = b.Id,
                   Name = b.Name,
                   TotalFloors = b.TotalFloors,
                   Distance = b.Distance,
                   NumberOfRooms = (int)b.NumberOfRooms,
                   CreatedDate = b.CreatedDate,
                   UpdatedDate = b.UpdatedDate,
                   ProvinceName = b.Province.Name,
                   DistrictName = b.District.Name,
                   WardName = b.Ward.Name,
                   AddressDetails = $"{b.Address.Information}",
                   BuildingStatus = b.BuildingStatus.Name,
                   LinkEmbedMap = b.LinkEmbedMap
               })
               .FirstOrDefaultAsync();

            if (building == null)
            {
                return NotFound("Building not found.");
            }

            return Ok(building);

        }

        [HttpGet("GetBuildingById/{id}")]
        public async Task<IActionResult> GetBuildingById(int id)
        {
            var building = await _context.Buildings
                .Include(b => b.Address)
                .Include(b => b.BuildingStatus)
                .Include(b => b.User)
                .Include(b => b.Province)
                .Include(b => b.District)
                .Include(b => b.Ward)
                .Where(b => b.Id == id)
                .Select(b => new BuildingDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    TotalFloors = b.TotalFloors,
                    NumberOfRooms = (int)b.NumberOfRooms,
                    Distance = b.Distance,
                    CreatedDate = b.CreatedDate,
                    UpdatedDate = b.UpdatedDate,
                    ProvinceName = b.Province.Name,
                    DistrictName = b.District.Name,
                    WardName = b.Ward.Name,
                    AddressDetails = $"{b.Address.Information}",
                    BuildingStatus = b.BuildingStatus.Name,
                    LinkEmbedMap = b.LinkEmbedMap
                })
                .FirstOrDefaultAsync();

            if (building == null)
            {
                return NotFound("Building not found.");
            }

            return Ok(building);
        }




        [HttpPut("EditBuilding/{id}")]
        public async Task<ActionResult<BuildingDTO>> EditBuildingById(int id, [FromBody] BuildingDTO buildingDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find the building by ID
            var building = await _context.Buildings
                .Include(b => b.Address)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (building == null)
            {
                return NotFound("Building not found.");
            }

            // Find Province
            var province = await _context.Provinces
                .FirstOrDefaultAsync(p => p.Name.Equals(buildingDto.ProvinceName));

            if (province == null)
            {
                return BadRequest($"Province '{buildingDto.ProvinceName}' not found.");
            }

            // Find District
            var district = await _context.Districts
                .FirstOrDefaultAsync(d => d.Name.Equals(buildingDto.DistrictName) && d.ProvincesId == province.Id);

            if (district == null)
            {
                return BadRequest($"District '{buildingDto.DistrictName}' not found in province '{buildingDto.ProvinceName}'.");
            }

            // Find Ward
            var ward = await _context.Wards
                .FirstOrDefaultAsync(w => w.Name.Equals(buildingDto.WardName) && w.DistrictId == district.Id);

            if (ward == null)
            {
                return BadRequest($"Ward '{buildingDto.WardName}' not found in district '{buildingDto.DistrictName}'.");
            }

            // Find BuildingStatus
            var buildingStatus = await _context.BuildingStatuses
                .FirstOrDefaultAsync(bs => bs.Name.Equals(buildingDto.BuildingStatus));

            if (buildingStatus == null)
            {
                return BadRequest($"Building status '{buildingDto.BuildingStatus}' not found.");
            }

            // Update the building details
            building.Name = buildingDto.Name;
            building.Distance = buildingDto.Distance;
            building.TotalFloors = buildingDto.TotalFloors;
            building.NumberOfRooms = buildingDto.NumberOfRooms;
            building.UpdatedDate = DateTime.UtcNow;
            building.ProvinceId = province.Id;
            building.DistrictId = district.Id;
            building.WardId = ward.Id;
            building.BuildingStatusId = buildingStatus.Id;
            building.LinkEmbedMap = buildingDto.LinkEmbedMap;

            // Update the address details (if address exists)
            if (building.Address != null)
            {
                building.Address.Information = buildingDto.AddressDetails;
                building.Address.DistrictId = district.Id;
                building.Address.WardId = ward.Id;
                building.Address.ProvinceId = province.Id;
            }
            else
            {
                // Handle the case if Address is null
                building.Address = new Address
                {
                    Information = buildingDto.AddressDetails,
                    DistrictId = district.Id,
                    WardId = ward.Id,
                    ProvinceId = province.Id
                };
                _context.Add(building.Address);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the updated building DTO
            return Ok(buildingDto);
        }


        [HttpGet("GetBuildingStatus")]
        public async Task<IActionResult> GetBuildingStaus()
        {
            var status = await _context.BuildingStatuses.ToListAsync();
            return Ok(status);
        }

    }
}