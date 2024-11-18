namespace RMS_API.DTOs
{
    public class RoomLlDTO
    {
        public decimal Price { get; set; }
        public string? RoomNumber { get; set; }
        public double Area { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Floor { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int BuildingId { get; set; }
        public int RoomStatusId { get; set; }
    }
}
