namespace RMS_API.DTOs
{
    public class ServiceRecordFilter
    {
        public int? BuildingId { get; set; }
        public int? RoomId { get; set; }
        public DateTime SignedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
    }
}
