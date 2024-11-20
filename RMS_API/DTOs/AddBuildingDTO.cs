namespace RMS_API.DTOs
{
    public class AddBuildingDTO
    {
        public string Name { get; set; } = null!;
        public int TotalFloors { get; set; }
        public int NumberOfRooms { get; set; }
        public double? Distance { get; set; }
        public string? LinkEmbedMap { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
        public string DistrictName { get; set; } = null!;
        public string WardName { get; set; } = null!;
        public string AddressDetails { get; set; } = null!;
        public string BuildingStatus { get; set; } = null!;
        public int UserId { get; set; }

    }
}