using System.ComponentModel.DataAnnotations;

namespace RMS_API.DTOs
{
    public class RegisterModel
    {
        [Required]
        [DataType(DataType.Text)]
        public string? FirstName { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string? LastName { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string? MidName { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string? Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string? Phone { get; set; }       
        public string? VerificationCode { get; set; }

    }
}
