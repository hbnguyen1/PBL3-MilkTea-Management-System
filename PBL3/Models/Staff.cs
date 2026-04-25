using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PBL3.Models
{
    public class Staff : Users
    {
        public int salaryPerHour { get; set; } = 0;
        public Boolean isAvailable { get; set; } = true;
        //[ForeignKey("userID")]
        //public virtual Users User { get; set; }
    }
}
