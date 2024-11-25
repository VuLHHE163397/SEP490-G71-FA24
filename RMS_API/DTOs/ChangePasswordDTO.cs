namespace RMS_API.DTOs
{
    public class ChangePasswordDTO
    {
        public string UserId { get; set; } = null!;
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
