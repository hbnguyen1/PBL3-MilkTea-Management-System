using PBL3.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Manangers
{
    internal class CustomerManagers
    {
        public static void Register()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Đăng ký tài khoản:");
            string name = Console.ReadLine();
            string phoneNumber = Console.ReadLine();
            string password = Console.ReadLine();
            CustomerService customerService = new CustomerService();
            bool isRegistered = customerService.AddNewCustomer(name, phoneNumber, password);
            if (isRegistered)
            {
                Console.WriteLine("Đăng ký thành công!");
            }
            else
            {
                Console.WriteLine("Đăng ký thất bại: Số điện thoại đã tồn tại. Vui lòng chọn số khác.");
            }
        }
    }
}
