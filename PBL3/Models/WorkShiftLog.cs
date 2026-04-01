using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class WorkShiftLog
    {
        public int id { get; set; }
        public int staffID { get; set; }

        public DateTime checkInTime { get; set; }
        public DateTime? checkOutTime { get; set; }

        public double totalHours { get; set; } = 0;
    }
}
