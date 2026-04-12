using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class Item
    {
        public int itemID { get; set; }
        public required string size { get; set; } = string.Empty;
        public required string itemName { get; set; } = string.Empty;
        public required string itemType { get; set; } = string.Empty;
        public Boolean isAvailable { get; set; } = true;
        public required double price { get; set; } = 0;

    }
}
