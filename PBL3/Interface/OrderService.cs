using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Data;
using PBL3.Models;
using PBL3.Interface;
using PBL3.Core;

namespace PBL3.Interface
{
    internal class OrderService : IOrderService
    {
        public bool CreateOrder(Orders order, List<OrderDetails> listorders)
        {
            using (var conn = new MilkTeaDBContext())
            {
                using (var transaction = conn.Database.BeginTransaction())
                {
                    try
                    {
                        conn.Orders.Add(order);
                        conn.SaveChanges();
                        Logger.Info($"Order created ID:{order.orderID} Total:{order.totalPrice}");

                        foreach (var item in listorders)
                        {
                            item.orderID = order.orderID;
                            conn.OrderDetails.Add(item);

                            var recipe = conn.Recipes
                                             .Where(r => r.itemID == item.itemID && r.size == item.size)
                                             .ToList();

                            foreach (var rep in recipe)
                            {
                                var ig = conn.Ingredients.Find(rep.ingredientID);

                                if (ig != null)
                                {
                                    ig.igCount -= (rep.quantityNeeded * item.quantity);

                                    Logger.Info($"Ingredient {ig.igName} -{rep.quantityNeeded * item.quantity}");
                                }
                            }
                        }

                        conn.SaveChanges();
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
                    // IN THẲNG LỖI RA MÀN HÌNH ĐỂ BẮT MA
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
}
