namespace RMS_API.DTOs
{
    public class RoomSearchDTO
    {
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public int WardId { get; set; }
        public double? MinDistance { get; set; }
        public double? MaxDistance { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public double? MinArea { get; set; }
        public double? MaxArea { get; set; }
        public string? RoomStatus { get; set; }
    }
}
