using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.DTOs;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceBillController : ControllerBase
    {
        private readonly RMS_SEP490Context context = new RMS_SEP490Context();
        [HttpGet]
        public IActionResult SearchBills([FromQuery] ServiceBillFilter filter)
        {
            try
            {
                var rooms = context.Rooms
                    .Include(e => e.Building).Include(e => e.Services)
                    .Where(e => filter.RoomId == null || filter.RoomId <= 0 || e.Id == filter.RoomId)
                    .Where(e => filter.BuildingId == null || filter.BuildingId <= 0 || e.BuildingId == filter.BuildingId)
                    .Where(e => e.UserId == filter.UserId)
                    .Where(e => (e.StartedDate == null) || filter.Date == null || (filter.Date.Value.Date >= e.StartedDate.Value.Date))
                    .Where(e => (e.ExpiredDate == null) || filter.Date == null || (filter.Date.Value.Date <= e.ExpiredDate.Value.Date))
                    .ToList();
                var roomIds = rooms.Select(e => e.Id).ToList();
                var serviceBills = context.ServicesBills
                    .Where(e => !roomIds.Any() || roomIds.Contains((int)e.RoomId!))
                    .ToList();
                var response = rooms.Select(e =>
                {
                    return new ServiceBillDTO
                    {
                        RoomId = e.Id,
                        RoomNumber = e.RoomNumber,
                        BuildingId = e.BuildingId,
                        BuildingName = e.Building.Name,
                        TotalPrice = e.Price + e.Services.Sum(service =>
                        {
                            if (service.Type == 1)
                            {
                                var serviceBill = service.ServicesBills
                                    .FirstOrDefault(s => s.ServiceId == service.Id); // Sử dụng FirstOrDefault để tránh lỗi null
                                return serviceBill != null ? serviceBill.Price : 0; // Xử lý nếu không tìm thấy
                            }
                            return service.Price; // Type khác 1, trả về giá gốc
                        }),

                        RemainingPrice = e.Price + serviceBills.Where(x => x.RoomId == e.Id).Sum(e => e.Price),
                    };
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Calculate(int RoomId)
        {
            try
            {
                var today = DateTime.Now;
                var addedServiceBills = await context.ServicesBills.Where(e => e.RoomId == RoomId && e.Date.Month == today.Month && e.Date.Year == today.Year)
                    .ToListAsync();
                var serices = await context.Services
                    .AsNoTracking()
                    .Include(e => e.Rooms)
                    .Where(e => e.Rooms.Select(r => r.Id).Contains(RoomId))
                    .ToListAsync();
                foreach (var calc in serices)
                {
                    var serivce = addedServiceBills.FirstOrDefault(e => e.ServiceId == calc.Id);
                    if (serivce == null && calc.Type != 1)
                    {
                        //var services = context.Services
                        //.Where(s => s.Rooms.Any(r => r.Id == RoomId))
                        //.Where(s => s.Type != 1)
                        //.Select(s => new
                        //{
                        //    s.Id,
                        //    s.Name,
                        //    s.Price
                        //})
                        //.ToList();
                        //var room = await context.Rooms.FirstOrDefaultAsync(e => e.Id == RoomId);
                        //if (room == null)
                        //{
                        //    return NotFound("Room not found");
                        //}
                        //services.ForEach(e =>
                        //{
                        //    var bill = new ServicesBill
                        //    {
                        //        Name = $"{e.Name} tháng {today.Month}/{today.Year}",
                        //        Date = today,
                        //        Price = e.Price,
                        //        ServiceId = e.Id,
                        //        RoomId = RoomId,
                        //    };
                        //    context.ServicesBills.Add(bill);
                        //});
                        var bill = new ServicesBill
                        {
                            Name = $"{calc.Name} tháng {today.Month}/{today.Year}",
                            Date = today,
                            Price = calc.Price,
                            ServiceId = calc.Id,
                            RoomId = RoomId,
                        };
                        context.ServicesBills.Add(bill);
                        await context.SaveChangesAsync();
                    }
                }

                return Ok(new { message = "Tính phí dịch vụ thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("Detail")]
        public IActionResult GetDetail(int RoomId, DateTime date)
        {
            try
            {
                var room = context.Rooms
                    .Include(e => e.Services)
                    .FirstOrDefault(e => e.Id == RoomId);
                var serviceBills = context.ServicesBills
                    .AsNoTracking()
                    .Where(e => e.RoomId == RoomId)
                    .Where(e => e.Date.Month == date.Month && e.Date.Year == date.Year)
                    .Include(e => e.Room)
                    .Include(e => e.Service)
                    .Where(e => room.Services.Select(e => e.Id).Contains(e.ServiceId))
                    .Select(e => new ServicesBillDTO
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Date = e.Date,
                        Price = e.Price,
                        ServiceId = e.Id,
                        RoomId = e.RoomId,
                        ServiceRecordId = e.Id,
                        ServiceType = e.Service.Type
                    })
                    .ToList();
                var lastElectriceService = serviceBills.Where(e => e.ServiceType == 1).OrderByDescending(e => e.Date).FirstOrDefault();
                serviceBills.RemoveAll(e => e.ServiceType == 1);
                if (lastElectriceService != null)
                {
                    serviceBills.Add(lastElectriceService);
                }
                serviceBills.Add(new ServicesBillDTO
                {
                    Name = "Tiền nhà",
                    Date = date,
                    Price = (decimal)room?.Price,
                });
                return Ok(serviceBills);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}