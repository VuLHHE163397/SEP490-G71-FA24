using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Building
    {
        public Building()
        {
            Rooms = new HashSet<Room>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int TotalFloors { get; set; }
        public double? Distance { get; set; }
        public string? LinkEmbedMap { get; set; }
        public int? NumberOfRooms { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public int WardId { get; set; }
        public int AddressId { get; set; }
        public int BuildingStatusId { get; set; }

        public virtual Address Address { get; set; } = null!;
        public virtual BuildingStatus BuildingStatus { get; set; } = null!;
        public virtual District District { get; set; } = null!;
        public virtual Province Province { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual Ward Ward { get; set; } = null!;
        public virtual ICollection<Room> Rooms { get; set; }
        public virtual ICollection<Service> Services { get; set; }
    }
}
