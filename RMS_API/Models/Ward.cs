using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Ward
    {
        public Ward()
        {
            Addresses = new HashSet<Address>();
            Buildings = new HashSet<Building>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int DistrictId { get; set; }

        public virtual District District { get; set; } = null!;
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Building> Buildings { get; set; }
    }
}
