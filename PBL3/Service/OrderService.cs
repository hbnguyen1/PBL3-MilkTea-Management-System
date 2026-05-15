using PBL3.Core;
using PBL3.Data;
using PBL3.GUI;
using PBL3.Interface;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PBL3.Service
{
    internal class OrderService : IOrderService
    {
        // 1. Khai báo biến bất biến (Chỉ đọc)
        private readonly MilkTeaDBContext _conn;

        // 2. Tiêm DbContext thông qua Constructor
        public OrderService(MilkTeaDBContext conn)
        {
            _conn = conn;
        }

        public bool CreateOrder(Orders order, List<OrderDetails> listorders)
        {
            // Xóa using var conn = new... Dùng trực tiếp _db
            using (var transaction = _conn.Database.BeginTransaction())
            {
                try
                {
                    _conn.Orders.Add(order);
                    _conn.SaveChanges();
                    Logger.Info($"Order created ID:{order.orderID} Total:{order.totalPrice}");

                    foreach (var item in listorders)
                    {
                        item.orderID = order.orderID;
                        _conn.OrderDetails.Add(item);

                        var recipe = _conn.Recipes
                                         .Where(r => r.itemID == item.itemID && r.size == item.size)
                                         .ToList();

                        foreach (var rep in recipe)
                        {
                            var ig = _conn.Ingredients.Find(rep.ingredientID);

                            if (ig != null)
                            {
                                ig.igCount -= (int)(rep.quantityNeeded * item.quantity);
                                Logger.Info($"Ingredient {ig.igName} -{rep.quantityNeeded * item.quantity}");
                            }
                        }
                    }

                    _conn.SaveChanges();
                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

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

        public int CreateNewOrder(int staffID, int? customerID, List<POSCartItem> cart, double total)
        {
            using (var transaction = _conn.Database.BeginTransaction())
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

                    _conn.Orders.Add(newOrder);
                    _conn.SaveChanges(); // Lấy được orderID tự tăng

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
                        _conn.OrderDetails.Add(detail);
                    }

                    _conn.SaveChanges();
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

        public bool ProcessNextOrderInQueue(int orderId, int staffid)
        {
            try
            {
                var order = _conn.Orders.FirstOrDefault(o => o.orderID == orderId);
                if (order != null)
                {
                    order.orderStatus = "Completed";
                    _conn.SaveChanges();
                    Logger.Info($"Nhân viên {staffid} duyệt đơn {orderId} thành công!");
                    return true;
                }
                else
                {
                    Logger.Error($"Đơn hàng {orderId} không tồn tại!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi khi duyệt đơn: {ex.Message}");
                return false;
            }
        }

        public Orders? GetNextOrder()
        {
            try
            {
                var nextOrder = _conn.Orders
                                    .Where(o => o.orderStatus == "Pending")
                                    .OrderBy(o => o.orderDate)
                                    .FirstOrDefault();
                return nextOrder;
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi khi lấy đơn: {ex.Message}");
                return null;
            }
        }

        public List<Orders> GetAllOrders()
        {
            return _conn.Orders.OrderByDescending(o => o.orderID).ToList();
        }

        public List<Orders> GetOrdersByStatus(string status)
        {
            return _conn.Orders.Where(o => o.orderStatus == status).OrderByDescending(o => o.orderID).ToList();
        }

        public List<Orders> GetPendingOrders()
        {
            try
            {
                var pendingOrders = _conn.Orders
                                        .Where(o => o.orderStatus == "Pending")
                                        .OrderBy(o => o.orderDate)
                                        .ToList();

                return pendingOrders;
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách đơn Pending: " + ex.Message);
                return new List<Orders>();
            }
        }

        public List<OrderDetails> GetOrderDetails(int orderId)
        {
            try
            {
                return _conn.OrderDetails.Where(d => d.orderID == orderId).ToList();
            }
            catch (Exception ex)
            {
                // Vẫn giữ lại dòng in log "bắt ma" cực chất của bạn :D
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
}