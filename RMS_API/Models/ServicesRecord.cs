using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class ServicesRecord
    {
        public ServicesRecord()
        {
            ServicesBills = new HashSet<ServicesBill>();
        }

        public int Id { get; set; }
        public int OldMeter { get; set; }
        public int NewMeter { get; set; }
        public int? TotalMeter { get; set; }
        public int ServiceId { get; set; }
        public int? RoomId { get; set; }

        public virtual Room? Room { get; set; }
        public virtual Service Service { get; set; } = null!;
        public virtual ICollection<ServicesBill> ServicesBills { get; set; }
    }
}
