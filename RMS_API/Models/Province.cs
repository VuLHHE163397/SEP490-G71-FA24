using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Province
    {
        public Province()
        {
            Addresses = new HashSet<Address>();
            Buildings = new HashSet<Building>();
            Districts = new HashSet<District>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Building> Buildings { get; set; }
        public virtual ICollection<District> Districts { get; set; }
    }
}
