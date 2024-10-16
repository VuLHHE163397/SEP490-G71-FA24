using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class District
    {
        public District()
        {
            Addresses = new HashSet<Address>();
            Buildings = new HashSet<Building>();
            Wards = new HashSet<Ward>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int ProvincesId { get; set; }

        public virtual Province Provinces { get; set; } = null!;
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Building> Buildings { get; set; }
        public virtual ICollection<Ward> Wards { get; set; }
    }
}
