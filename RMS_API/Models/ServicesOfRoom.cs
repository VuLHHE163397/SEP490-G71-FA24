using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class ServicesOfRoom
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int ServiceId { get; set; }

        public virtual Room Room { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}
