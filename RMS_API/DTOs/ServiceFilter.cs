namespace RMS_API.DTOs
{
    public class ServiceFilter
    {
        public string? keyword { get; set; }
        public int buildingId { get; set; } = 0;
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
    }
}
