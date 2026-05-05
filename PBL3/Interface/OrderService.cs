using System;
using System.Collections.Generic;
using System.Linq;
using PBL3.Data;
using PBL3.Models;
using PBL3.Core;

namespace PBL3.Interface
{
    internal class OrderService : IOrderService
    {
        private readonly CustomerPointService _pointService;

        public OrderService()
        {
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
                        conn.Orders.Add(order);
                        conn.SaveChanges();
                        Logger.Info($"Order tạo ID:{order.orderID} Tổng tiền:{order.totalPrice}");

                        var itemIds = listorders.Select(i => i.itemID).Distinct().ToList();

                        var allRecipes = conn.Recipes
                            .Where(r => itemIds.Contains(r.itemID))
                            .ToList();

                        var ingredientIds = allRecipes
                            .Select(r => r.ingredientID)
                            .Distinct()
                            .ToList();

                        var ingredients = conn.Ingredients
                            .Where(i => ingredientIds.Contains(i.igID))
                            .ToDictionary(i => i.igID);

                        foreach (var item in listorders)
                        {
                            item.orderID = order.orderID;

                            var recipe = allRecipes
                                .Where(r => r.itemID == item.itemID && r.size == item.size)
                                .ToList();

                            double costForOneCup = 0;

                            foreach (var rep in recipe)
                            {
                                if (ingredients.ContainsKey(rep.ingredientID))
                                {
                                    var ig = ingredients[rep.ingredientID];

                                    ig.igCount -= (rep.quantityNeeded * item.quantity);

                                    Logger.Info($"Ingredient {ig.igName} - {rep.quantityNeeded * item.quantity}");

                                    costForOneCup += rep.quantityNeeded * ig.price;
                                }
                            }

                            item.costAtOrder = costForOneCup;
                            conn.OrderDetails.Add(item);
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
                            Console.WriteLine("SQL: " + ex.InnerException.Message);

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

                    if (order != null && order.orderStatus == "Pending")
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

                    Console.WriteLine("Đơn hàng này đã được xử lý trước đó rồi!");
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
                        .Where(o => o.orderStatus != null && o.orderStatus.Trim().ToLower() == "pending")
                        .OrderBy(o => o.orderDate)
                        .ToList();

                    Console.WriteLine($"[DEBUG]: Lấy được {pendingOrders.Count} đơn Pending");

                    return pendingOrders;
                }
                catch (Exception ex)
                {
                    Logger.Error("Lỗi khi lấy đơn Pending: " + ex.Message);
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
                    return conn.OrderDetails
                        .Where(d => d.orderID == orderId)
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi chi tiết đơn {orderId}: " + ex.Message);
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
                    Logger.Error($"Lỗi lịch sử đơn {customerId}: " + ex.Message);
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
                    Logger.Error($"Lỗi lấy đơn {orderId}: " + ex.Message);
                    return null;
                }
            }
        }
    }
}