namespace RMS_API.DTOs
{
    public class RoomDTO
    {
        public int Id { get; set; }
        public string Building { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double Area { get; set; }
        public string RoomStatusName { get; set; } = string.Empty;
        public double? Distance { get; set; }
        public string? FirstImageLink { get; set; }
    }
}
