using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class Orders
    {
        public  int orderID { get; set; } = 0;
        public required int customerID{ get; set; } = 0;
        public int? staffID { get; set; } = 0;
        public required DateTime orderDate { get; set; }
        public required string orderStatus { get; set; } = string.Empty;
        public required double totalPrice { get; set; } = 0;
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
    }
}
