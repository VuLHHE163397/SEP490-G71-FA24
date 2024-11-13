using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RMS_API.Models;

namespace RMS_Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacilityStatusController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;
        public FacilityStatusController()
        {
            _context = new RMS_SEP490Context();
        }
        [HttpGet("GetAllFacility")]
        public IActionResult Search()
        {
            try
            {
                var data = _context.FacilitiesStatuses.ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
