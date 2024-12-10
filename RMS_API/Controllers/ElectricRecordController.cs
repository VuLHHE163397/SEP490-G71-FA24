using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.DTOs;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricRecordController : ControllerBase
    {
        private readonly RMS_SEP490Context context = new RMS_SEP490Context();
        [HttpGet]
        public IActionResult Search([FromQuery] ServiceRecordFilter filter)
        {
            try
            {
                var rooms = context.Rooms
                    .Include(e => e.Building)
                    .Where(e => filter.RoomId == null || filter.RoomId <= 0 || e.Id == filter.RoomId)
                    .Where(e => filter.BuildingId == null || filter.BuildingId <= 0 || e.BuildingId == filter.BuildingId)
                    .Where(e => e.UserId == filter.UserId)
                    .Where(e => (e.StartedDate == null) ||  (filter.SignedDate.Date >= e.StartedDate.Value.Date))
                    .Where(e => (e.ExpiredDate == null) ||  (filter.SignedDate.Date <= e.ExpiredDate.Value.Date))
                    .ToList();
                var roomIds = rooms.Select(e => e.Id).ToList();
                var serviceRecords = context.ServicesRecords
                    .Where(e => roomIds.Contains((int)e.RoomId!))
                    .OrderByDescending(e => e.NewMeter)
                    .ToList();
                var response = rooms.Select(e =>
                {
                    var oldMeter = serviceRecords.Where(x => x.RoomId == e.Id)
                               .FirstOrDefault()?.NewMeter;
                    return new ElectricRecordDTO
                    {
                        BuildingId = e.BuildingId,
                        BuildingName = e.Building.Name,
                        RoomId = e.Id,
                        RoomNumber = e.RoomNumber,
                        OldMeter = oldMeter != null ? (int)oldMeter : 0,
                        NewMeter = oldMeter != null ? (int)oldMeter : 0,
                        TotalMeter = 0
                    };
                }).ToList();
                return Ok(response);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public IActionResult Create([FromBody] ElectricRecordDTO dto)
        {
            try
            {
                //Mặc định 1 là điện
                var electricService = context.Services.Where(e => e.BuildingId == dto.BuildingId && e.Type == 1)
                    .FirstOrDefault();
                var serviceBill = context.ServicesBills
                    .Where(e => e.RoomId == dto.RoomId)
                    .Include(e => e.ServiceRecord)
                    .OrderByDescending(e => e.Date)
                    .FirstOrDefault();
                var serviceRecords = context.ServicesRecords.Where(e => e.RoomId == dto.RoomId)
                    .OrderByDescending(e => e.NewMeter)
                    .FirstOrDefault();
                if(serviceBill != null)
                {
                    if (serviceBill.Date.Date > dto.RecordDate.Date)
                    {
                        throw new Exception("Ngày chốt sổ gần nhất là " + serviceBill.Date.ToString("dd/MM/yyyy") + ". Ngày chốt sổ mới phải lớn hơn ngày chốt sổ gần nhất");
                    }
                }
                if(serviceRecords != null)
                {
                    if (serviceRecords.NewMeter != dto.OldMeter)
                    {
                        throw new Exception("Số điện cũ không khớp");
                    }
                }
                if (dto.OldMeter > dto.NewMeter)
                {
                    throw new Exception("Số điện mới phải lớn hoặc bằng số điện cũ");
                }
                if(serviceBill != null && serviceBill.Date.Date == dto.RecordDate.Date)
                {
                    serviceBill.ServiceRecord.NewMeter = dto.NewMeter;
                    serviceBill.ServiceRecord.TotalMeter = dto.NewMeter - serviceBill.ServiceRecord.OldMeter;
                    serviceBill.Price = electricService.Price * (serviceBill.ServiceRecord.TotalMeter ?? 0);
                    context.ServicesBills.Update(serviceBill);
                    context.SaveChanges();
                    return Ok();
                }
                else
                {
                    if (electricService != null)
                    {
                        var serviceRecord = new ServicesRecord
                        {
                            OldMeter = dto.OldMeter,
                            NewMeter = dto.NewMeter,
                            TotalMeter = dto.NewMeter - dto.OldMeter,
                            ServiceId = electricService.Id,
                            RoomId = dto.RoomId,
                        };
                        context.ServicesRecords.AddRange(serviceRecord);
                        context.SaveChanges();
                        var serviceBillNew = new ServicesBill
                        {
                            Name = $"Điện tháng {dto.RecordDate.Month}/{dto.RecordDate.Year}",
                            Date = dto.RecordDate,
                            Price = (decimal)(electricService.Price * serviceRecord.TotalMeter),
                            ServiceId = electricService.Id,
                            ServiceRecordId = serviceRecord.Id,
                            RoomId = dto.RoomId,
                        };
                        context.ServicesBills.Add(serviceBillNew);
                        context.SaveChanges();
                        return Ok();
                    }
                    else
                    {
                        return NotFound("Không có dịch vụ điện");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
