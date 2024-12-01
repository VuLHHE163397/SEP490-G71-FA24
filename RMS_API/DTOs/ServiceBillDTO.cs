namespace RMS_API.DTOs
{
    public class ServiceBillDTO
    {
        public int BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public int RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string? GuestName {  get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Payment { get; set; } = 0;
        public decimal RemainingPrice { get; set; } = 0;
    }
}
