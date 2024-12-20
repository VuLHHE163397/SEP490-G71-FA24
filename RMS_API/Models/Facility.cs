﻿using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int FacilityStatusId { get; set; }
        public int? RoomId { get; set; }
        public int? UserId { get; set; }

        public virtual FacilitiesStatus FacilityStatus { get; set; } = null!;
        public virtual Room? Room { get; set; }
    }
}
