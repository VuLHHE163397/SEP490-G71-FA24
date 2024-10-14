using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class FacilitiesOfRoom
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int FacilityId { get; set; }

        public virtual Facility Facility { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
    }
}
