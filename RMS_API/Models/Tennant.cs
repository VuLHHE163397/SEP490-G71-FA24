using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Tennant
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Phone { get; set; }
        public string Email { get; set; } = null!;
        public int RoomId { get; set; }

        public virtual Room Room { get; set; } = null!;
    }
}
