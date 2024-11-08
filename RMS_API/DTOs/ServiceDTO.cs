using System.ComponentModel.DataAnnotations;

namespace RMS_API.DTOs
{
    public class ServiceDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên dịch vụ không được trống")]
        [MaxLength(50, ErrorMessage = "Tên dịch vụ không được vượt quá 50 kí tự")]
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "Giá dịch vụ không được trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ không được nhỏ hơn 0")]
        public decimal Price { get; set; }

        
    }
}
