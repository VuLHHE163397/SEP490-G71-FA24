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
                    throw new Exception("Service not found");
                }
                return Ok(facilities);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetAllFacility")]
        public async Task<ActionResult<IEnumerable<FacilityDTO>>> GetAllFacilities()
        {
            var facilities = await _context.Facilities
                 .Include(f => f.FacilityStatus)
                .Select(f => new FacilityDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    RoomId = f.RoomId,
                    FacilityStatusName = f.FacilityStatus.Description
                })
                .ToListAsync();

            return Ok(facilities);
        }

        //update dịch vụ
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(FacilityDTO facilityDTO)
        {
            try
            {
                var facilityDb = await _context.Facilities.FirstOrDefaultAsync(e => e.Id == facilityDTO.Id);
                if (facilityDb == null)
                {
                    throw new Exception("Facility not found");
                }
                facilityDb.Name = facilityDTO.Name;
                facilityDb.RoomId = facilityDTO.RoomId;
                facilityDb.FacilityStatus = facilityDTO.FacilityStatus;
                _context.Facilities.Update(facilityDb);
                await _context.SaveChangesAsync();
                return Ok(facilityDb);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
