using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class Staff : Users
    {
        public required int salary { get; set; } = 0;
        public required string workShift { get; set; } = string.Empty;
    }
}
