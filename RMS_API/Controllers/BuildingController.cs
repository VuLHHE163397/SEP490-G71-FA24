using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.DTOs;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingController : ControllerBase
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
                .Include(b => b.Rooms)
                .Include(b => b.Address) // Include Address to fetch related data
                .Include(b => b.BuildingStatus) // Include BuildingStatus to fetch related data
                .Select(b => new BuildingDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    TotalFloors = b.TotalFloors,
                    NumberOfRooms = b.NumberOfRooms,
                    Address = $"{b.Address.Ward.Name}, {b.Address.District.Name}, {b.Address.Province.Name}", // Convert Address to string
                    BuildingStatus = b.BuildingStatus.Name // Convert BuildingStatus to string
                }).ToList();

            return Ok(buildings);
        }
    }
}
