using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos
{
    public class JourneyDto
    {
        public int OwnedBy { get; set; }
        public string StartGPS { get; set; } = string.Empty;
        public string EndGPS { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime FinishedAt { get; set; }
    }
}
