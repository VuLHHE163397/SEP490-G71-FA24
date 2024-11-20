namespace RMS_API.DTOs
{
    public class ServicesBillDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int ServiceId { get; set; }
        public int? ServiceRecordId { get; set; }
        public int? RoomId { get; set; }
    }
}
