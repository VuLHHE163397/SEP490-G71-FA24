using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class FacilitiesStatus
    {
        public FacilitiesStatus()
        {
            Facilities = new HashSet<Facility>();
        }

        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public int Status { get; set; }

        public virtual ICollection<Facility> Facilities { get; set; }
    }
}
