using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;

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
    }
}
