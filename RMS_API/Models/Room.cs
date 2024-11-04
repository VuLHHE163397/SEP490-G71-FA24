using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Room
    {
        public Room()
        {
            Facilities = new HashSet<Facility>();
            Images = new HashSet<Image>();
            MaintainanceRequests = new HashSet<MaintainanceRequest>();
            RoomHistories = new HashSet<RoomHistory>();
            Tennants = new HashSet<Tennant>();
        }

        public int Id { get; set; }
        public decimal Price { get; set; }
        public int RoomNumber { get; set; }
        public double Area { get; set; }
        public string Description { get; set; } = null!;
        public int Floor { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int BuildingId { get; set; }
        public int RooomStatusId { get; set; }

        public virtual Building Building { get; set; } = null!;
        public virtual RoomStatus RooomStatus { get; set; } = null!;
        public virtual ICollection<Facility> Facilities { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<MaintainanceRequest> MaintainanceRequests { get; set; }
        public virtual ICollection<RoomHistory> RoomHistories { get; set; }
        public virtual ICollection<Tennant> Tennants { get; set; }
    }
}
