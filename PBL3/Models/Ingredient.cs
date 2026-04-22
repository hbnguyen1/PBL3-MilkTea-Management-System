using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class Ingredient
    {
        public int igID { get; set; }
        public required string igName { get; set; } = string.Empty;
        public required int igCount { get; set; } = 0;
        public required double price { get; set; } = 0;
        public required string unit { get; set; } = string.Empty;
    }
}
