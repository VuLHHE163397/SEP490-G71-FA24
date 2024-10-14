using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class ServicesBill
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int ServiceId { get; set; }
        public int ServiceRecordId { get; set; }

        public virtual Service Service { get; set; } = null!;
        public virtual ServicesRecord ServiceRecord { get; set; } = null!;
    }
}
