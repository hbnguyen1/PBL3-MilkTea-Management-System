using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    internal class CustomerService: ICustomerService
    {
        public bool AddNewCustomer(string name, string phoneNumber, string password)
        {
            // Kiểm tra xem số điện thoại đã tồn tại trong database chưa
            using (var conn = new Data.MilkTeaDBContext())
            {
                var existingUser = conn.Users.SingleOrDefault(u => u.Phone == phoneNumber);
                if (existingUser != null)
                {
                    //Console.WriteLine("Số điện thoại đã tồn tại. Vui lòng chọn số khác.");
                    return false;
                }
                // Nếu chưa tồn tại, tạo mới khách hàng và lưu vào database
                var newCustomer = new Customer
                {
                    Name = name,
                    Phone = phoneNumber,
                    Password = password,
                    point = 0
                };
                conn.Customers.Add(newCustomer);
                conn.SaveChanges();
                //Console.WriteLine("Đăng ký thành công!");
                return true;
            }
        }
        public List<string> GetTrendingItemNamesForCustomer()
        {
            using (var db = new MilkTeaDBContext())
            {
                // 1. Chỉ gom nhóm và đếm số lượng để tìm ra món Hot
                // 2. Không hề đụng tới phép tính Doanh thu ở đây
                var trendingNames = db.OrderDetails
                    .GroupBy(od => od.itemID)
                    .OrderByDescending(g => g.Sum(od => od.quantity)) // Vẫn lấy top bán chạy
                    .Take(5)
                    .Select(g => db.Items.FirstOrDefault(i => i.itemID == g.Key).itemName) // CHỈ SELECT ĐÚNG TÊN MÓN
                    .ToList();

                return trendingNames;
            }
        }
    }
}
