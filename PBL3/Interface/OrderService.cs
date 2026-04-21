using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PBL3.Data;
using PBL3.Models;
using PBL3.Interface;
using PBL3.Core;

namespace PBL3.Interface
{
    internal class OrderService : IOrderService
    {
        // Khai báo Service điểm thưởng để dùng khi Duyệt đơn
        private readonly CustomerPointService _pointService;

        public OrderService()
        {
            // Trong Console App, khởi tạo trực tiếp ở đây cho tiện
            _pointService = new CustomerPointService();
        }

        public bool CreateOrder(Orders order, List<OrderDetails> listorders)
        {
            using (var conn = new MilkTeaDBContext())
            {
                using (var transaction = conn.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Lưu Order trước để tự sinh ra order.orderID
                        conn.Orders.Add(order);
                        conn.SaveChanges();
                        Logger.Info($"Order tạo ID:{order.orderID} Tổng tiền:{order.totalPrice}");

                        // 2. Xử lý từng chi tiết hóa đơn
                        foreach (var item in listorders)
                        {
                            item.orderID = order.orderID;

                            // Tìm công thức của ly nước này
                            var recipe = conn.Recipes
                                             .Where(r => r.itemID == item.itemID && r.size == item.size)
                                             .ToList();

                            double costForOneCup = 0; // Biến tính tổng giá vốn cho 1 ly

                            foreach (var rep in recipe)
                            {
                                var ig = conn.Ingredients.Find(rep.ingredientID);

                                if (ig != null)
                                {                       
                                    ig.igCount -= (rep.quantityNeeded * item.quantity);
                                    Logger.Info($"Ingredient {ig.igName} - {rep.quantityNeeded * item.quantity}");
                                    // Giá vốn 1 ly = Tổng (Định lượng * Đơn giá nhập)
                                    costForOneCup += rep.quantityNeeded * ig.price;
                                }
                            }
                            // Ghi nhận giá vốn 1 ly vào chi tiết đơn hàng để tính lợi nhuận
                            item.costAtOrder = costForOneCup;
                            conn.OrderDetails.Add(item);
                        }

                        //Save và kết thúc transaction
                        conn.SaveChanges();
                        transaction.Commit();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); //run it back

                        Console.WriteLine("\n--- PHÁT HIỆN LỖI NGHIÊM TRỌNG ---");
                        Console.WriteLine("Lỗi chung: " + ex.Message);

                        if (ex.InnerException != null)
                        {
                            Console.WriteLine("LỖI CHI TIẾT TỪ SQL SERVER: " + ex.InnerException.Message);
                        }

                        Console.WriteLine("----------------------------------\n");
                        Logger.Error(ex.Message);
                        return false;
                    }
                }
            }
        }

        public bool ApproveOrder(int orderID, int staffID)
        {
            using (var conn = new MilkTeaDBContext())
            {
                try
                {
                    var order = conn.Orders.Find(orderID);

                    if (order != null)
                    {
                        if (order.orderStatus == "Pending")
                        {
                            order.staffID = staffID;
                            order.orderStatus = "Approved";
                            conn.SaveChanges();
                            Logger.Info($"Nhân viên {staffID} đã duyệt đơn hàng số {orderID}");

                            if (order.customerID != null)
                            {
                                _pointService.AddPoints(order.customerID, order.totalPrice);
                            }

                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Đơn hàng này đã được xử lý trước đó rồi!");
                            return false;
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi khi duyệt đơn {orderID}: {ex.Message}");
                    return false;
                }
            }
        }

        public List<Orders> GetPendingOrders()
        {
            using (var conn = new MilkTeaDBContext())
            {
                try
                {
                    var pendingOrders = conn.Orders
                        // [ĐÃ SỬA]: Ép cắt khoảng trắng và đưa về chữ thường để so sánh bất chấp lỗi
                        .Where(o => o.orderStatus != null && o.orderStatus.Trim().ToLower() == "pending")
                        .OrderBy(o => o.orderDate)
                        .ToList();

                    // IN RA LOG ĐỂ KIỂM TRA CHÉO
                    Console.WriteLine($"[TEST DEBUG]: EF Core vừa móc lên được {pendingOrders.Count} đơn Pending!");

                    return pendingOrders;
                }
                catch (Exception ex)
                {
                    Logger.Error("Lỗi khi lấy danh sách đơn Pending: " + ex.Message);
                    return new List<Orders>();
                }
            }
        }

        public List<OrderDetails> GetOrderDetails(int orderId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                try
                {
                    return conn.OrderDetails.Where(d => d.orderID == orderId).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n[BẮT ĐƯỢC MA EF CORE]: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine("Chi tiết: " + ex.InnerException.Message);
                    }

                    Logger.Error($"Lỗi khi lấy chi tiết đơn {orderId}: " + ex.Message);
                    return new List<OrderDetails>();
                }
            }
        }

        public List<Orders> GetOrderHistoryForCustomer(int customerId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                try
                {
                    return conn.Orders
                               .Where(o => o.customerID == customerId)
                               .OrderByDescending(o => o.orderDate)
                               .ToList();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi khi lấy lịch sử đơn hàng cho khách {customerId}: " + ex.Message);
                    return new List<Orders>();
                }
            }
        }

        public Orders GetOrdersById(int orderId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                try
                {
                    return conn.Orders.Find(orderId);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi khi lấy đơn hàng ID {orderId}: " + ex.Message);
                    return null;
                }
            }
        }
    }
}