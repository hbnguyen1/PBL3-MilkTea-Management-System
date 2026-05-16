using PBL3.GUI;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace PBL3.Interface
{
    public interface IOrderService
    {
        bool CreateOrder(Orders order, List<OrderDetails> listorders);
        public int CreateNewOrder(int staffID, int? customerID, List<POSCartItem> cart, double total);
        bool ProcessNextOrderInQueue(int orderId, int staffid);
        Orders? GetNextOrder();
        Orders? GetOrderById(int id);
        List<Orders> GetAllOrders();
        List<Orders> GetOrdersByStatus(string status);
        List<Orders> GetPendingOrders();
        List<OrderDetails> GetOrderDetails(int orderID);
    }
}
