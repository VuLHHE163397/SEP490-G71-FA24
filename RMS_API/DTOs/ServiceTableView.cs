namespace RMS_API.DTOs
{
    public class ServiceTableView
    {
        public List<ServiceDTO> services { get; set; } = new List<ServiceDTO>();
        public int TotalRecord {  get; set; }
    }
}
