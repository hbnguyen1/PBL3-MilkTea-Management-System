using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class SalarySummary
    {
        public int id { get; set; }
        public int staffID { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public double totalHours { get; set; }
        public int penalty { get; set; }
        public double totalSalary { get; set; }
    }
}
