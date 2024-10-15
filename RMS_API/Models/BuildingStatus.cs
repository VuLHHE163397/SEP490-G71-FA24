using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class BuildingStatus
    {
        public BuildingStatus()
        {
            Buildings = new HashSet<Building>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Building> Buildings { get; set; }
    }
}
