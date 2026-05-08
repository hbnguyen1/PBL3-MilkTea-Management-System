using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class Staff : Users
    {
        public required int salaryPerHour { get; set; } = 0;

        public virtual ICollection<WorkShiftLog> WorkShiftLogs { get; set; } = new List<WorkShiftLog>();
        public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();
    }
}
