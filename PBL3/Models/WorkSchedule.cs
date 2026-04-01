using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class WorkSchedule
    {
        public int id { get; set; }
        public int staffID { get; set; }
        public DateTime workDate { get; set; }
        public string shift { get; set; } = string.Empty;
    }
}
