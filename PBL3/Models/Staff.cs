using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class Staff : Users
    {
        public required int salaryPerHour { get; set; } = 0;
    }
}
