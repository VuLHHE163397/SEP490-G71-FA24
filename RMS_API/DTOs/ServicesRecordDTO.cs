namespace RMS_API.DTOs
{
    public class ServicesRecordDTO
    {
        public int Id { get; set; }
        public int OldMeter { get; set; }
        public int NewMeter { get; set; }
        public int? TotalMeter { get; set; }
        public int ServiceId { get; set; }
        public int? RoomId { get; set; }

    }
}
