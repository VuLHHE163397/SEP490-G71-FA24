using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Report
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
    }
}
