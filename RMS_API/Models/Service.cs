using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Service
    {
        public Service()
        {
            ServicesBills = new HashSet<ServicesBill>();
            ServicesRecords = new HashSet<ServicesRecord>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }

        public virtual ICollection<ServicesBill> ServicesBills { get; set; }
        public virtual ICollection<ServicesRecord> ServicesRecords { get; set; }
    }
}
