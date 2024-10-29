using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.Models;


namespace RMS_API.Controllers
{
    [Route("api/Room")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public RoomController(RMS_SEP490Context context)
        {
            _context = context;
        }


        [HttpGet("getProvinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.Provinces.ToListAsync();
            return Ok(provinces);
        }

        [HttpGet("getDistricts/{provinceId}")]
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            var districts = await _context.Districts
                .Where(d => d.ProvincesId == provinceId)
                .ToListAsync();
            return Ok(districts);
        }

        [HttpGet("getWards/{districtId}")]
        public async Task<IActionResult> GetWards(int districtId)
        {
            var wards = await _context.Wards
                .Where(w => w.DistrictId == districtId)
                .ToListAsync();
            return Ok(wards);
        }

        [HttpGet("ListRoom")]
        public async Task<ActionResult<IEnumerable<Room>>> GetAvailableRooms()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Building) // Đưa thông tin về Building
                .Include(b => b.Building.Address) // Đưa thông tin về Address
                .Include(r => r.RooomStatus) // Đưa thông tin về RoomStatus
                .Where(r => r.RooomStatusId == 1) // Lọc phòng đang trống
                .ToListAsync();

            return Ok(rooms);
        }



    }
}
