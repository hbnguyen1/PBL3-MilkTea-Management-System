using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PBL3.Models
{
    public class WorkShiftLog
    {
        
        [Key] public int logID { get; set; }
        public int staffID { get; set; }
        public DateTime workDate { get; set; }
        public string shift { get; set; } = string.Empty;

        public DateTime? checkIn { get; set; }
        public DateTime? checkOut { get; set; }

        public double totalHours { get; set; } = 0;
        public int penalty { get; set; } = 0;
    }
}
