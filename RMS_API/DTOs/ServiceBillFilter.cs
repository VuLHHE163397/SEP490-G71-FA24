namespace RMS_API.DTOs
{
    public class ServiceBillFilter
    {
        public DateTime? Date {  get; set; }
        public int? RoomId { get; set; }
        public int? BuildingId { get; set; }
        public int UserId { get; set; }
    }
}
