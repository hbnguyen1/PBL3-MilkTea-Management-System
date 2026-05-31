using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.src.Domain.Models
{
    public class WorkSchedule
    {
        public int id { get; set; }
        public int staffID { get; set; }
        public DateTime workDate { get; set; }
        public string shift { get; set; } = string.Empty;
        public virtual Staff Staff { get; set; }
    }
}
