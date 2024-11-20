namespace RMS_API.Models
{
    public class ServicesOfRoom
    {
        public int RoomId { get; set; }
        public int ServiceId { get; set; }

        public virtual Room Room { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}
