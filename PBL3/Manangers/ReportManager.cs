using PBL3.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Manangers
{
    internal class ReportManager
    {
        public void ShowTopSellingReport()
        {
            ReportService reportService = new ReportService();
            var topList = reportService.GetTopSellingItems(5);

            Console.Clear();
            Console.WriteLine("        BÁO CÁO: TOP 5 MÓN BÁN CHẠY NHẤT            ");
            Console.WriteLine("{0,-5} | {1,-20} | {2,-10} | {3,-12}", "Top", "Tên món", "Số lượng", "Doanh thu");
            int rank = 1;
            foreach (var item in topList)
            {
                Console.WriteLine("{0,-5} | {1,-20} | {2,-10} | {3,-12:N0}đ",
                    rank++,
                    item.ItemName,
                    item.TotalQuantity + " ly",
                    item.TotalRevenue);
            }
            Console.WriteLine("\nNhấn phím bất kỳ để quay lại...");
            Console.ReadKey();
        }
    }
}
