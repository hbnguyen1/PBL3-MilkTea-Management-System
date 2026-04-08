using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PBL3.Models
{
    public class Customer : Users
    {
        public required int point { get; set; } = 0;
    }
}
