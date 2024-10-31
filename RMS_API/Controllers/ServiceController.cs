using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.DTOs;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public ServiceController(RMS_SEP490Context context)
        {
            _context = context;
        }



        [HttpGet("GetAllService")]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetAllServices()
        {
            var services = await _context.Services
                .Select(s => new ServiceDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price
                })
                .ToListAsync();

            return Ok(services);
        }







    }
}


