using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class User
    {
        public User()
        {
            Buildings = new HashSet<Building>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string MidName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public int UserStatusId { get; set; }
        public int RoleId { get; set; }
<<<<<<< Updated upstream
        public string? FacebookUrl { get; set; }
=======
        public string? FbUrl { get; set; }
>>>>>>> Stashed changes
        public string? ZaloUrl { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual UserStatus UserStatus { get; set; } = null!;
        public virtual ICollection<Building> Buildings { get; set; }
    }
}
