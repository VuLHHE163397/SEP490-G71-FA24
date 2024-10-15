using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Address
    {
        public Address()
        {
            Buildings = new HashSet<Building>();
        }

        public int Id { get; set; }
        public string Information { get; set; } = null!;
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public int WardId { get; set; }

        public virtual District District { get; set; } = null!;
        public virtual Province Province { get; set; } = null!;
        public virtual Ward Ward { get; set; } = null!;
        public virtual ICollection<Building> Buildings { get; set; }
    }
}
