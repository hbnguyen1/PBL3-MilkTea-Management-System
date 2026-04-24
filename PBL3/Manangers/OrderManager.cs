using System;
using System.Collections.Generic;
using PBL3.Models;
using PBL3.Interface;

namespace PBL3.Manangers
{
    internal class OrderManager
    {
        public void Order(Users currentUser)
        {
            OrderService orderService = new OrderService();
        }
        public List<Orders> GetAllOrders()
        {
            OrderService orderService = new OrderService();
            return orderService.GetAllOrders();
        }

        public List<Orders> GetOrdersByStatus(string status)
        {
            OrderService orderService = new OrderService();
            return orderService.GetOrdersByStatus(status);
        }
    }
}