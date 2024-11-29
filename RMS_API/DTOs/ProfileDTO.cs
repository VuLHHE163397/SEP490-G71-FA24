namespace RMS_API.DTOs
{
    public class ProfileDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string MidName { get; set; } = null!;
        public string Email { get; set; } = null!;       
        public string Phone { get; set; } = null!;
        public string? FacebookUrl { get; set; }
        public string? ZaloUrl { get; set; }
    }
}