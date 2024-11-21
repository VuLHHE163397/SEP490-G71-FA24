namespace RMS_API.DTOs
{
    public class SuggestedRoomDTO
    {
        public string? RoomNumber { get; set; }
        public int Id { get; set; }
        public decimal Price { get; set; }
        public double Area { get; set; }
        public string RoomStatusName { get; set; } = string.Empty;
        public string? FirstImageLink { get; set; }
    }
}
