﻿using RMS_API.Models;
using System.ComponentModel.DataAnnotations;
namespace RMS_API.DTOs
{
    public class FacilityDTO
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int FacilityStatusId { get; set; }
        public int? RoomId { get; set; }
        public string FacilityStatusName { get; set; } 


        public virtual FacilitiesStatus FacilityStatus { get; set; } = null!;
        public virtual Room? Room { get; set; }
    }
}

