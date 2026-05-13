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
        bool ProcessNextOrderInQueue(int orderId, int staffid);
        Orders? GetNextOrder();
        List<Orders> GetAllOrders();
        List<Orders> GetOrdersByStatus(string status);
        List<Orders> GetPendingOrders();
        List<OrderDetails> GetOrderDetails(int orderID);
    }
}
