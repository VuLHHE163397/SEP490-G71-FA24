using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.DTOs;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Landlord")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public ServiceController(RMS_SEP490Context context)
        {
            _context = context;
        }

        [HttpGet]

        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                var services = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
                if (services == null)
                {
                    throw new Exception("Service not found");
                }
                return Ok(services);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Landlord")]
        //lấy danh sách dịch vụ
        [HttpGet("GetAllService")]

        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetAllServices([FromQuery] ServiceFilter filter)
        {
            var services = await _context.Services
                .Where(e => e.UserId == filter.userId)
                .Include(e => e.Building)
                .Select(s => new ServiceDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    BuildingName = s.Building.Name,
                    BuildingId = s.Building.Id,
                    Price = s.Price
                })
                .ToListAsync();
            int total = services.Count;
            services = services.Where(e => filter.buildingId <= 0 || e.BuildingId == filter.buildingId)
                .ToList();

            services = services.Skip((filter.pageIndex - 1) * filter.pageSize)
                .Take(filter.pageSize)
                .ToList();
            return Ok(new ServiceTableView
            {
                TotalRecord = total,
                services = services
            });
        }

        //add dịch vụ
        [HttpPost]
        public async Task<IActionResult> CreateAsync(ServiceDTO serviceDTO)
        {
            try
            {
                if (serviceDTO.BuildingId == null)
                {
                    throw new Exception("Building not found");
                }
                var service = new Service
                {
                    Name = serviceDTO.Name,
                    Price = serviceDTO.Price,
                    UserId = serviceDTO.UserId,
                    BuildingId = (int)serviceDTO.BuildingId,
                    Type = (int)serviceDTO.type
                };
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                return Ok(service);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //update dịch vụ
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(ServiceDTO serviceDTO)
        {
            try
            {
                var serviceDb = await _context.Services.FirstOrDefaultAsync(e => e.Id == serviceDTO.Id);
                if (serviceDb == null)
                {
                    throw new Exception("Service not found");
                }
                serviceDb.Name = serviceDTO.Name;
                serviceDb.Price = serviceDTO.Price;
                _context.Services.Update(serviceDb);
                await _context.SaveChangesAsync();
                return Ok(serviceDb);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //Xóa dịch vụ
        [HttpDelete]
        [Authorize(Roles = "Landlord")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var serviceDb = await _context.Services.FirstOrDefaultAsync(e => e.Id == id);
                if (serviceDb == null)
                {
                    throw new Exception("Service not found");
                }
                _context.Services.Remove(serviceDb);
                await _context.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        ////lấy danh sách thanh toán dịch vụ
        //[HttpGet("GetServicesBill")]
        //public async Task<IActionResult> GetServicesBill()
        //{

        //}
    }
}


