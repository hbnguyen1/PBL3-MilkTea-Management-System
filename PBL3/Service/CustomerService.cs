using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;
using BCrypt.Net;
using PBL3.Interface;

namespace PBL3.Service
{
    internal class CustomerService: ICustomerService
    {
        private readonly MilkTeaDBContext _conn;
        public CustomerService(MilkTeaDBContext conn)
        {
            _conn = conn;
        }
        public bool AddNewCustomer(string name, string phoneNumber, string password)
        {
                var existingUser = _conn.Users.SingleOrDefault(u => u.Phone == phoneNumber);
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
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    point = 0
                };
                _conn.Customers.Add(newCustomer);
                _conn.SaveChanges();
                //Console.WriteLine("Đăng ký thành công!");
                return true;
        }
        public List<string> GetTrendingItemNamesForCustomer()
        {
                // 1. Chỉ gom nhóm và đếm số lượng để tìm ra món Hot
                // 2. Không hề đụng tới phép tính Doanh thu ở đây
                var trendingNames = _conn.OrderDetails
                    .GroupBy(od => od.itemID)
                    .OrderByDescending(g => g.Sum(od => od.quantity)) // Vẫn lấy top bán chạy
                    .Take(5)
                    .Select(g => _conn.Items.FirstOrDefault(i => i.itemID == g.Key).itemName) // CHỈ SELECT ĐÚNG TÊN MÓN
                    .ToList();

                return trendingNames;        }
    }
}
