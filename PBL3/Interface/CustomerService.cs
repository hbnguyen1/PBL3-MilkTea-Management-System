using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PBL3.Interface
{
    internal class CustomerService : ICustomerService
    {
        public bool AddNewCustomer(string name, string phoneNumber, string password)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var existingUser = conn.Users.SingleOrDefault(u => u.Phone == phoneNumber);
                if (existingUser != null)
                {
                    Console.WriteLine("Số điện thoại đã tồn tại. Vui lòng chọn số khác.");
                    return false;
                }

                var newCustomer = new Customer
                {
                    Name = name,
                    Phone = phoneNumber,
                    Password = password,
                    point = 0
                };

                conn.Customers.Add(newCustomer);
                conn.SaveChanges();

                Console.WriteLine("Đăng ký thành công!");
                return true;
            }
        }

        public List<string> GetTrendingItemNamesForCustomer()
        {
            using (var db = new MilkTeaDBContext())
            {
                var topItemIds = db.OrderDetails
                    .GroupBy(od => od.itemID)
                    .Select(g => new
                    {
                        Id = g.Key,
                        Total = g.Sum(x => x.quantity)
                    })
                    .OrderByDescending(x => x.Total)
                    .Take(5)
                    .Select(x => x.Id)
                    .ToList();

                if (!topItemIds.Any())
                {
                    return new List<string>();
                }
                var itemDict = db.Items
                    .Where(i => topItemIds.Contains(i.itemID))
                    .ToDictionary(i => i.itemID, i => i.itemName);
                var trendingNames = new List<string>();

                foreach (var id in topItemIds)
                {
                    if (itemDict.ContainsKey(id))
                    {
                        trendingNames.Add(itemDict[id]);
                    }
                }

                return trendingNames;
            }
        }
    }
}