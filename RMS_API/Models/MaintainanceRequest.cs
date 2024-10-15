using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class MaintainanceRequest
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public DateTime RequestDate { get; set; }
        public int Status { get; set; }
        public int RoomId { get; set; }

        public virtual Room Room { get; set; } = null!;
    }
}
