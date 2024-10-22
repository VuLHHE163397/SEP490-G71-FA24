namespace RMS_API.DTOs
{
    public class AddBuidingDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int TotalFloors { get; set; }
        public int NumberOfRooms { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Foreign key properties

       
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public int WardId { get; set; }
        public int BuildingStatusId { get; set; } // Add this line

        // Display properties
        public string ProvinceName { get; set; } = null!;
        public string DistrictName { get; set; } = null!;
        public string WardName { get; set; } = null!;
        public string AddressDetails { get; set; } = null!;
        public string BuildingStatus { get; set; } = null!;

    }
}
