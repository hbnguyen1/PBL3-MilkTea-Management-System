using System;
using System.Collections.Generic;
using System.Linq; // Thêm thư viện này để dùng được .Where và .ToList
using System.Text;
using PBL3.Data;
using PBL3.Models;
using PBL3.Interface;

namespace PBL3.Interface
{
    internal class OrderService : IOrderService
    {
        // 1. ĐỔI KIỂU TRẢ VỀ TỪ bool SANG int
        public int CreateOrder(Orders order, List<OrderDetails> listorders)
        {
            using (var conn = new MilkTeaDBContext())
            {
                using (var transaction = conn.Database.BeginTransaction())
                {
                    try
                    {
                        conn.Orders.Add(order);

                        // Sau dòng SaveChanges này, Entity Framework cực kỳ thông minh, 
                        // nó sẽ tự động lấy ID thực tế dưới SQL gắn vào order.orderID
                        conn.SaveChanges();

                        foreach (var item in listorders)
                        {
                            item.orderID = order.orderID; // Dùng luôn ID vừa được tạo
                            conn.OrderDetails.Add(item);
                            var recipe = conn.Recipes
                                             .Where(r => r.itemID == item.itemID && r.size == item.size)
                                             .ToList();

                            foreach (var rep in recipe)
                            {
                                var ig = conn.Ingredients.Find(rep.ingredientID);
                                if (ig != null)
                                {
                                    ig.igCount -= ((int)rep.quantityNeeded * item.quantity);
                                }
                            }
                        }
                        conn.SaveChanges();
                        transaction.Commit();

                        // 2. TRẢ VỀ MÃ ĐƠN HÀNG VỪA TẠO
                        return order.orderID;
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

                        // 3. TRẢ VỀ 0 NẾU GẶP LỖI
                        return 0;
                    }
                }
            }
        }
    }
}