﻿namespace RMS_API.DTOs
{
    public class RoomLlUpdateDTO
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string? RoomNumber { get; set; }
        public double Area { get; set; }
        public string Description { get; set; } = null!;
        public int Floor { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int BuildingId { get; set; }
        public int RoomStatusId { get; set; }
        public DateTime? FreeInFutureDate { get; set; }
    }
}
