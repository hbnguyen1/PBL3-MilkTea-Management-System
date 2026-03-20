using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class OrderDetails
    {
        public  int orderID { get; set; }
        public required int itemID { get; set; } = 0;
        public required string size { get; set; } = string.Empty;
        public required int quantity { get; set; } = 0;
        public required int priceAtOrder { get; set; } = 0;
        public required string note { get; set; } = string.Empty;
    }
}
