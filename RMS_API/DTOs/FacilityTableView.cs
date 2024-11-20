namespace RMS_API.DTOs
{
    public class FacilityTableView
    {
        public List<FacilityDTO> Facilities { get; set; } = new List<FacilityDTO>();
        public int Total { get; set; } = 0;
    }
}
