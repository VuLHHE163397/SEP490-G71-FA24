﻿using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Numbers { get; set; }
        public int FacilityStatusId { get; set; }

        public virtual FacilitiesStatus FacilityStatus { get; set; } = null!;
    }
}
