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

            return Json(districts); // Trả về dưới dạng JSON
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



        [HttpPost("AddBuilding")]
        public async Task<IActionResult> AddBuilding([FromBody] AddBuildingDTO buildingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Tìm Province
            var province = await _context.Provinces
                .FirstOrDefaultAsync(p => p.Name.Equals(buildingDto.ProvinceName));

            if (province == null)
            {
                return BadRequest($"Tinh/Thanh pho '{buildingDto.ProvinceName}' not found.");
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

            // Tìm hoặc tạo địa chỉ mới
            var address = new Address
            {
                Information = buildingDto.AddressDetails,
                DistrictId = district.Id,
                WardId = ward.Id,
                ProvinceId = province.Id
            };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            // Tạo đối tượng Building mới
            var building = new Building
            {
                Name = buildingDto.Name,
                TotalFloors = buildingDto.TotalFloors,
                NumberOfRooms = buildingDto.NumberOfRooms,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                ProvinceId = province.Id,
                DistrictId = district.Id,
                WardId = ward.Id,
                AddressId = address.Id,
                BuildingStatusId = buildingStatus.Id,
                UserId = 2

            };

            // Thêm tòa nhà vào cơ sở dữ liệu
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            return Ok(buildingDto);
        }

        [HttpGet("AddBuilding")]
        public IActionResult AddBuilding()
        {
            // Lấy danh sách các tỉnh
            ViewBag.Provinces = _context.Provinces.ToList();

            // Lấy danh sách trạng thái tòa nhà
            ViewBag.BuildingStatuses = _context.BuildingStatuses.ToList();

            return View(); // Trả về view cho việc thêm tòa nhà
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
                    CreatedDate = b.CreatedDate,
                    UpdatedDate = b.UpdatedDate,
                    ProvinceName = b.Province.Name,
                    DistrictName = b.District.Name,
                    WardName = b.Ward.Name,
                    AddressDetails = $"{b.Address.Ward.Name}, {b.Address.District.Name}, {b.Address.Province.Name}",
                    BuildingStatus = b.BuildingStatus.Name
                })
                .FirstOrDefaultAsync();

            if (building == null)
            {
                return NotFound("Building not found.");
            }

            return Ok(building);
        }

        [HttpPut("EditBuilding/{id}")]
        public async Task<IActionResult> EditBuilding(int id, [FromBody] AddBuildingDTO buildingDto)
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
            building.TotalFloors = buildingDto.TotalFloors;
            building.NumberOfRooms = buildingDto.NumberOfRooms;
            building.UpdatedDate = DateTime.UtcNow;
            building.ProvinceId = province.Id;
            building.DistrictId = district.Id;
            building.WardId = ward.Id;
            building.BuildingStatusId = buildingStatus.Id;

            // Update the address details
            if (building.Address != null)
            {
                building.Address.Information = buildingDto.AddressDetails;
                building.Address.DistrictId = district.Id;
                building.Address.WardId = ward.Id;
                building.Address.ProvinceId = province.Id;
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok("Building updated successfully.");
        }



    }
}
