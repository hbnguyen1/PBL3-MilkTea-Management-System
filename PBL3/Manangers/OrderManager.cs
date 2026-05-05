using PBL3.Data; // Thêm thư viện này để gọi DB
using PBL3.GUI;
using PBL3.Interface;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PBL3.Manangers
{
    internal class OrderManager
    {
        // Hàm mới để lưu Hóa đơn từ giao diện POS
        // Sửa kiểu trả về từ bool thành int
        public int CreateNewOrder(int staffID, int? customerID, List<POSCartItem> cart, double total)
        {
            using (var db = new PBL3.Data.MilkTeaDBContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var newOrder = new Orders
                        {
                            staffID = staffID,
                            customerID = customerID ?? 0,
                            orderDate = DateTime.Now,
                            totalPrice = total,
                            orderStatus = "Pending"
                        };

                        db.Orders.Add(newOrder);
                        db.SaveChanges(); // Lấy được orderID tự tăng

                        foreach (var item in cart)
                        {
                            var detail = new OrderDetails
                            {
                                orderID = newOrder.orderID,
                                itemID = item.itemID,
                                size = item.size,
                                quantity = item.quantity,
                                note = item.note, // Đã bổ sung lưu Ghi chú (Đá, Đường)
                                priceAtOrder = item.price
                            };
                            db.OrderDetails.Add(detail);
                        }

                        db.SaveChanges();
                        transaction.Commit();
                        return newOrder.orderID; // Trả về mã đơn để in hóa đơn
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Lỗi tạo đơn: " + ex.Message);
                        return -1; // Thất bại trả về -1
                    }
                }
            }
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