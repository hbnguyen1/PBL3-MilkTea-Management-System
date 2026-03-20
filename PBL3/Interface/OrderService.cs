using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Data;
using PBL3.Models;
using PBL3.Interface;

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
                                }
                            }
                        }
                        conn.SaveChanges();
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Nếu bạn đang dùng Transaction thì nhớ để lại dòng transaction.Rollback(); nhé
                        Console.WriteLine("\n--- PHÁT HIỆN LỖI NGHIÊM TRỌNG ---");
                        Console.WriteLine("Lỗi chung: " + ex.Message);

                        // ĐÂY LÀ CHÌA KHÓA: Moi lỗi từ tận đáy SQL Server ra
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine("LỖI CHI TIẾT TỪ SQL SERVER: " + ex.InnerException.Message);
                        }
                        Console.WriteLine("----------------------------------\n");
                        return false;
                    }
                }
            }
        }
    }
}
