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
    }
}
