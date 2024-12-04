namespace RMS_API.DTOs
{
    public class RoomStatsDTO
    {
        public string BuildingName { get; set; }
        public int AvailableRooms { get; set; }
        public int RentedRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public int ExpiringRooms { get; set; }
    }
}
