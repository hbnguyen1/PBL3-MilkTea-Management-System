using System;
using System.Collections.Generic;
using System.Text;
using BCrypt.Net;
using PBL3.src.Application.Interface;
using PBL3.src.Infrastructure.Data;
using PBL3.src.Domain.Models;

namespace PBL3.src.Application.Service
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
                    return false;
                }
                //Nếu chưa tồn tại, tạo mới khách hàng và lưu vào database
                var newCustomer = new Customer
                {
                    Name = name,
                    Phone = phoneNumber,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    point = 0
                };
                _conn.Customers.Add(newCustomer);
                _conn.SaveChanges();
                return true;
        }
        public List<string> GetTrendingItemNamesForCustomer()
        {
                //Chỉ gom nhóm và đếm số lượng để tìm ra món Hot
                var trendingNames = _conn.OrderDetails
                    .GroupBy(od => od.itemID)
                    .OrderByDescending(g => g.Sum(od => od.quantity)) //Lấy top bán chạy
                    .Take(5)
                    .Select(g => _conn.Items.FirstOrDefault(i => i.itemID == g.Key).itemName) //SELECT ĐÚNG TÊN MÓN
                    .ToList();

                return trendingNames;        
        }
    }
}
