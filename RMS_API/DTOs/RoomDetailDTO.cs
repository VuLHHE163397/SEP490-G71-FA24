namespace RMS_API.DTOs
{
    public class RoomDetailDTO
    {
        public string Building { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string FullAddress { get; set; }
        public DateTime? FreeInFutureDate { get; set; }
        public decimal Price { get; set; }
        public double Area { get; set; }
        public double? Distance { get; set; }
        public string Description { get; set; }
        public string LinkEmbedMap { get; set; }
        public string RoomStatus { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
        public string? Facebook { get; set; }
        public string? Zalo { get; set; }
        public List<ImageDTO> Images { get; set; }
    }
}
