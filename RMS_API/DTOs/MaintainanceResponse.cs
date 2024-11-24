namespace RMS_API.DTOs
{
    public class MaintainanceResponse
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? RoomNumber { get; set; }
        public string? BuildingName { get; set; }
        public string? Address { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? SolveDate { get; set; }
        public string? MaintenanceDescription { get; set; }
    }
}
