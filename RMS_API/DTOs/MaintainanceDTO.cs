namespace RMS_API.DTOs
{
    public class MaintainanceDTO
    {
        public string Description { get; set; } = string.Empty;
        public DateTime newDate { get; set; }
        public int Status { get; set; }
        public int RoomId { get; set; }
    }
}
