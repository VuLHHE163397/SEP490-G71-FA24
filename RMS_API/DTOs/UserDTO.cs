namespace RMS_API.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string MidName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public int UserStatusId { get; set; }
        public int RoleId { get; set; }

        public string UserStatusName { get; set; }
        public string RoleName { get; set; }

    }
}
