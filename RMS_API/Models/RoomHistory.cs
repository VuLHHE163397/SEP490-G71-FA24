using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class RoomHistory
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RoomId { get; set; }

        public virtual Room Room { get; set; } = null!;
    }
}
