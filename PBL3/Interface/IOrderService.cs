using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using PBL3.Models;

namespace PBL3.Interface
{
    internal interface IOrderService
    {
        bool CreateOrder(Orders order, List<OrderDetails> listorders);
        bool ApproveOrder(int orderID, int staffID);
        List<Orders> GetPendingOrders();
        List<OrderDetails> GetOrderDetails(int orderID);
        public interface IOrderService
        {
            Orders ProcessCheckout(int customerId, List<CartItemDTO> cartItems);
        }
        public class CartItemDTO
        {
            public int ItemId { get; set; }
            public string Size { get; set; }
            public int Quantity { get; set; }
            public string Note { get; set; }
        }
    }
}
