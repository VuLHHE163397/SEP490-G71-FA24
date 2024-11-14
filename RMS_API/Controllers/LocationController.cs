using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public LocationController(RMS_SEP490Context context)
        {
            _context = context;
        }

        // API lấy danh sách các tỉnh
        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.Provinces
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();
            return Ok(provinces);
        }

        // API lấy danh sách huyện dựa trên Id của tỉnh
        [HttpGet("districts/{provinceId}")]
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            var districts = await _context.Districts
                .Where(d => d.ProvincesId == provinceId)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();
            return Ok(districts);
        }

        // API lấy danh sách xã dựa trên Id của huyện
        [HttpGet("wards/{districtId}")]
        public async Task<IActionResult> GetWards(int districtId)
        {
            var wards = await _context.Wards
                .Where(w => w.DistrictId == districtId)
                .Select(w => new { w.Id, w.Name })
                .ToListAsync();
            return Ok(wards);
        }
    }
}
