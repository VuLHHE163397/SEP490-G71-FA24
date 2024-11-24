using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RMS_API.DTOs;
using RMS_API.Helper;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Landlord")]
    public class MaintainanceRequestController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public MaintainanceRequestController(RMS_SEP490Context context)
        {
            _context = context;
        }

        [HttpGet("getMaintainedByUserId/{status}")]
        public IActionResult GetListMaintainance(int status)
        {
            string token = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(token)) return BadRequest("Invalid token");
            if (token.StartsWith("Bearer "))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }
            int? userId = UserHelper.GetUserIdFromToken(token);
            //if (int.IsNullOrEmpty(userId)) return Unauthorized("Invalid User");
            if(userId == null)
            {
                return BadRequest("User not exsit");
            }

            var maintainances = _context.MaintainanceRequests
                .Include(r => r.Room)
                    .ThenInclude(b => b.Building)
                .Where(mr => mr.Room.Building.UserId == userId && mr.Status == status)
                .OrderByDescending(o => o.RequestDate)
                .Select(mr => new MaintainanceResponse
                {
                    Id = mr.Id,
                    MaintenanceDescription = mr.Description,
                    RequestDate = mr.RequestDate,
                    SolveDate = mr.SolveDate,
                    Status = mr.Status == 1 ? "Chưa xử lý" : "Đã xử lý",
                    RoomNumber = mr.Room.RoomNumber,
                    BuildingName = mr.Room.Building.Name,
                    Address = mr.Room.Building.Address.Information + " " + mr.Room.Building.Ward.Name 
                    + " " + mr.Room.Building.District.Name + " " + mr.Room.Building.Province.Name
                }).ToList();
            if(!maintainances.Any())
            {
                return NotFound();
            }

        return Ok(maintainances);
        }

        [HttpPost("ChangeMaintainace/{id}")]
        public IActionResult ChangeMaintainaceStatusById(int? id)
        {
            if(id == null)
            {
                return BadRequest("Id đang null: " + id);
            }

            var maintainance = _context.MaintainanceRequests
                .FirstOrDefault(mr => mr.Id == id);

            if(maintainance == null)
            {
                return NotFound("Không tìm thấy mã bảo trì nào với id: " + id);
            }else if(maintainance.Status == 0)
            {
                maintainance.Status = 1;
                maintainance.SolveDate = null;
            }
            else
            {
                maintainance.Status = 0;
                maintainance.SolveDate = DateTime.Now;
            }
            var errordate = maintainance;
            _context.SaveChanges();
            return Ok(maintainance);
        }
    }
}
