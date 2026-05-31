using System;
using System.Collections.Generic;

namespace PBL3.src.Domain.Models
{
    public class OrderDetails
    {
        public  int orderID { get; set; }
        public required int itemID { get; set; } = 0;
        public required string size { get; set; } = string.Empty;
        public required int quantity { get; set; } = 0;
        public required double priceAtOrder { get; set; } = 0;
        public required string? note { get; set; } = string.Empty;
        //public double? costAtOrder { get; set; }
        public virtual Orders Order { get; set; }
        public virtual Item Item { get; set; }
    }
}
