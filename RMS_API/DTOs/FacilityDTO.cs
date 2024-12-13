using RMS_API.Models;
using System.ComponentModel.DataAnnotations;
namespace RMS_API.DTOs
{
    public class FacilityDTO
    {

        public int? Id { get; set; }

        [Required(ErrorMessage = "Chưa nhập tên cơ sở vật chất")]
        [MaxLength(50, ErrorMessage = "Tên cơ sở vật chất không được vượt quá 50 kí tự")]
        public string Name { get; set; } = null!;

        
        [Required(ErrorMessage = "Vui lòng chọn phòng.")]
        public int? RoomId { get; set; }
        public string? RoomNumber { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn tình trạng.")]
        public int? statusId {  get; set; }
        public string? FacilityStatus { get; set; } = null!;
        public int? UserId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn nhà.")]
        public int? BuildingId { get; set; }
        public string? BuildingName { get; set; } = null!;




    }
}


