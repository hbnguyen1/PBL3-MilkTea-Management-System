using System;
using System.Collections.Generic;
using PBL3.Models;
using PBL3.Interface;

namespace PBL3.Manangers
{
    internal class OrderManager
    {
        // Hàm Order này sẽ được viết lại sau để nhận danh sách món ăn từ Giao diện Giỏ hàng (WPF)
        // thay vì dùng Console.ReadLine() như trước đây.
        public void Order(Users currentUser)
        {
            OrderService orderService = new OrderService();
            // TODO: Viết logic xử lý đơn hàng từ WPF truyền xuống
        }
    }
}