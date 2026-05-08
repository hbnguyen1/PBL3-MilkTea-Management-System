using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PBL3.Models
{
    public class Orders
    {
        public  int orderID { get; set; } = 0;
        public required int customerID{ get; set; } = 0;
        [ForeignKey(nameof(Staff))]
        public int? staffID { get; set; } = 0;
        public required DateTime orderDate { get; set; }
        public required string orderStatus { get; set; } = string.Empty;
        public required double totalPrice { get; set; } = 0;

        public virtual Staff? Staff { get; set; }
    }
}
