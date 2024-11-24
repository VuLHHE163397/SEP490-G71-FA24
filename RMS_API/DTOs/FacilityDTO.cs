using RMS_API.Models;
using System.ComponentModel.DataAnnotations;
namespace RMS_API.DTOs
{
    public class FacilityDTO
    {

        public int? Id { get; set; }
        public string Name { get; set; } = null!;
       
        public int? RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public int? statusId {  get; set; }
        public string? FacilityStatus { get; set; } = null!;
        public int? UserId { get; set; }

    }
}


