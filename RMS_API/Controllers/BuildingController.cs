using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
                    NumberOfRooms = b.NumberOfRooms,
                    CreatedDate = b.CreatedDate,
                    UpdatedDate = b.UpdatedDate,
                    ProvinceName = b.Province.Name,
                    DistrictName = b.District.Name,
                    WardName = b.Ward.Name,
                    AddressDetails = $"{b.Address.Ward.Name}, {b.Address.District.Name}, {b.Address.Province.Name}",
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

        /*[HttpPost("AddBuilding")]
        public async Task<IActionResult> AddBuilding([FromBody] AddBuidingDTO addBuildingDTO)
        {
            if (addBuildingDTO == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Tạo đối tượng Building từ DTO
            var building = new Building
            {
                Name = addBuildingDTO.Name,
                TotalFloors = addBuildingDTO.TotalFloors,
                NumberOfRooms = addBuildingDTO.NumberOfRooms,
                // Chỉ sử dụng các thuộc tính có trong mô hình
                BuildingStatus = addBuildingDTO.BuildingStatus,
                // Nếu không có WardName và DistrictName trong mô hình, bạn có thể bỏ qua chúng
                // Nếu cần thiết, có thể lưu trữ các giá trị này trong một bảng khác hoặc một cách khác
            };

            try
            {
                // Thêm tòa nhà vào cơ sở dữ liệu
                await _context.Buildings.AddAsync(building);
                await _context.SaveChangesAsync();

                return Ok("Tòa nhà đã được thêm thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Có lỗi xảy ra khi thêm tòa nhà: {ex.Message}");
            }
        }*/


        [HttpGet("AddBuilding")]
        public IActionResult AddBuilding()
        {
            // Lấy danh sách các tỉnh
            ViewBag.Provinces = _context.Provinces.ToList();

            // Lấy danh sách trạng thái tòa nhà
            ViewBag.BuildingStatuses = _context.BuildingStatuses.ToList();

            return View(); // Trả về view cho việc thêm tòa nhà
        }

        // Thêm phương thức GET cho AddBuilding
        
    }
}
