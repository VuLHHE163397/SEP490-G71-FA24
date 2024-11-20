using System;
using System.Collections.Generic;

namespace RMS_API.Models
{
    public partial class Image
    {
        public int Id { get; set; }
        public string Link { get; set; } = null!;
        public int RoomId { get; set; }

        public virtual Room Room { get; set; } = null!;
    }
}
