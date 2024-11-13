using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.DTOs;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacilityController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public FacilityController(RMS_SEP490Context context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                var facilities = await _context.Facilities.FirstOrDefaultAsync(f => f.Id == id);
                if (facilities == null)
                {
                    throw new Exception("Facility not found");
                }
                return Ok(facilities);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetAllFacility")]
        public async Task<ActionResult<IEnumerable<FacilityDTO>>> GetAllFacilities([FromQuery] FacilityFilter filter)
        {
            var facilities = await _context.Facilities
                 .Include(f => f.FacilityStatus)
                .Select(f => new FacilityDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    RoomNumber = f.Room.RoomNumber,
                    FacilityStatus = f.FacilityStatus.Description,
                })
                .Where(e => filter.roomId <= 0 || e.RoomId == filter.roomId)
                .Skip((filter.pageIndex - 1) * filter.pageSize)
                .Take(filter.pageSize)
                .ToListAsync();
            if(filter.keyword != null)
            {
                filter.keyword = filter.keyword.Trim().ToLower();
                facilities = facilities.Where(e => e.Name.ToLower().Contains(filter.keyword) || e.FacilityStatus.ToLower().Contains(filter.keyword)).ToList();
            }
            return Ok(facilities);
        }

        //thêm cơ sở vật chất
        [HttpPost]
        public async Task<IActionResult> CreateAsync(FacilityDTO facilityDTO)
        {
            if (facilityDTO.statusId == null || facilityDTO.statusId <= 0)
            {
                return BadRequest($"Facility status '{facilityDTO.FacilityStatus}' not found.");
            }
            try
            {
                var facility = new Facility
                {
                    Name = facilityDTO.Name,
                    RoomId = facilityDTO.RoomId,
                    FacilityStatusId = (int)facilityDTO.statusId!,
                };
                _context.Facilities.Add(facility);
                await _context.SaveChangesAsync();
                return Ok(facility);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //update dịch vụ
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(FacilityDTO facilityDTO)
        {
            try
            {
                var facilityDb = await _context.Facilities.FirstOrDefaultAsync(f => f.Id == facilityDTO.Id);
                if (facilityDb == null)
                {
                    throw new Exception("Facility not found");
                }
                facilityDb.Name = facilityDTO.Name;
                facilityDb.RoomId = facilityDTO.RoomId;
                facilityDb.FacilityStatusId = (int)facilityDTO.statusId!;
                _context.Facilities.Update(facilityDb);
                await _context.SaveChangesAsync();
                return Ok(facilityDb);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Xóa cơ sở vật chất
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var facilityDb = await _context.Facilities.FirstOrDefaultAsync(e => e.Id == id);
                if (facilityDb == null)
                {
                    throw new Exception("Facility not found");
                }
                _context.Facilities.Remove(facilityDb);
                await _context.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
