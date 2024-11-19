namespace RMS_API.DTOs
{
    public class BillDTO
    {

        public int BillId { get; set; }
        public string BuildingName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        public string TenantName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
