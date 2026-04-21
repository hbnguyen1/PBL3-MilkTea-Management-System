using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Interface;
using PBL3.Models;

namespace PBL3.Manangers
{
    internal class ProfitManager
    {
        public void ShowProfitMenu()
        {
            ProfitService profitService = new ProfitService(new RevenueService());
            Console.WriteLine("===== BÁO CÁO LỢI NHUẬN =====");
            Console.WriteLine("1. Lợi nhuận theo ngày");
            Console.WriteLine("2. Lợi nhuận theo tháng");
            Console.WriteLine("3. Lợi nhuận theo năm");
            Console.WriteLine("4. Lợi nhuận theo khoảng thời gian");
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    //profitService.
                    break;
                case "2":
                    // Gọi hàm tính lợi nhuận theo tháng
                    break;
                case "3":
                    // Gọi hàm tính lợi nhuận theo năm
                    break;
                case "4":
                    // Gọi hàm tính lợi nhuận theo khoảng thời gian
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ!");
                    break;
            }
        }
    }
}
