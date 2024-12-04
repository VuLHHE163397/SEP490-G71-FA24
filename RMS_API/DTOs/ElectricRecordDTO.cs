namespace RMS_API.DTOs
{
    public class ElectricRecordDTO
    {
        public int BuildingId { get; set; }
        public string? BuildingName { get; set; } = null!;
        public int RoomId { get; set; }
        public string? RoomNumber { get; set; } = null!;
        public DateTime RecordDate { get; set; } = DateTime.Now;
        public int OldMeter {  get; set; }
        public int NewMeter { get; set; }
        public int TotalMeter { get; set; }
        public int? ServiceId { get; set; }
    }
}
