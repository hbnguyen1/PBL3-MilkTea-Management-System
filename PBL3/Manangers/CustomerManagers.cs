using PBL3.Interface;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PBL3.Manangers
{
    internal class CustomerManagers
    {
        public static void Register(string name, string phoneNumber, string password)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Đăng ký tài khoản:");
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
        public void ShowBestSeller()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            CustomerService customerService = new CustomerService();
            var trendingNames = customerService.GetTrendingItemNamesForCustomer();
            Console.WriteLine("Top 5 món bán chạy nhất:");
            for (int i = 0; i < trendingNames.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {trendingNames[i]}");
            }
        }
        public void CheckRank(Users customer)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            CustomerPointService customerpointService = new CustomerPointService();
            string rank = customerpointService.GetCustomerRank(customer.userID);
            int points = customerpointService.GetCurrentPoints(customer.userID);
            Console.WriteLine($"Hạng của khách hàng: {rank}");
            Console.WriteLine($"Số điểm hiện tại: {points}");
        }
        public void ShowHistoryOrder(Users customer)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            OrderService orderService = new OrderService();
            var orderHistory = orderService.GetOrderHistoryForCustomer(customer.userID);
            Console.WriteLine("Lịch sử đơn hàng:");
            foreach (var order in orderHistory)
            {
                Console.WriteLine($"- Đơn hàng ID: {order.orderID}, Ngày: {order.orderDate}, Tổng tiền: {order.totalPrice}");
            }
        }
    }
}
