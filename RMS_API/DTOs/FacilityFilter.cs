namespace RMS_API.DTOs
{
    public class FacilityFilter
    {
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
        public string? keyword { get; set; }
        public int buildingId { get; set; } = 0;
        public int roomId { get; set; } = 0;
    }
}
