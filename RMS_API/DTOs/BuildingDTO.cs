namespace RMS_API.DTOs
{
    public class BuildingDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int TotalFloors { get; set; }
        public int NumberOfRooms { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Foreign key properties
      
        // Display properties
        public string ProvinceName { get; set; } = null!;
        public string DistrictName { get; set; } = null!;
        public string WardName { get; set; } = null!;
        public string AddressDetails { get; set; } = null!;
        public string BuildingStatus { get; set; } = null!;
    }
}
