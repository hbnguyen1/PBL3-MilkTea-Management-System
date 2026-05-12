using PBL3.Data;
using PBL3.Interface;
using System;
using System.Globalization;

namespace PBL3.Manangers
{
    internal class RevenueManager
    {
        public void ShowRevenueMenu()
        {
            RevenueService revenueService = new RevenueService();

            Console.WriteLine("===== BÁO CÁO DOANH THU =====");
            Console.WriteLine("1. Doanh thu theo ngày");
            Console.WriteLine("2. Doanh thu theo tháng");
            Console.WriteLine("3. Doanh thu theo năm");
            Console.WriteLine("4. Doanh thu theo khoảng thời gian");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Nhập ngày (dd/MM/yyyy): ");
                    DateTime day;
                    while (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out day))
                    {
                        Console.WriteLine("Lỗi: Ngày không hợp lệ hoặc sai định dạng!");
                        Console.Write("Vui lòng nhập lại (dd/MM/yyyy): ");
                    }

                    double daily = revenueService.GetDailyRevenue(day);
                    Console.WriteLine($"Doanh thu ngày {day:dd/MM/yyyy}: {daily:N0} VNĐ");
                    break;

                case "2":
                    Console.Write("Nhập tháng: ");
                    int month = int.Parse(Console.ReadLine());

                    Console.Write("Nhập năm: ");
                    int year = int.Parse(Console.ReadLine());

                    double monthly = revenueService.GetRevenueByMonth(month, year);

                    Console.WriteLine($"Doanh thu tháng {month}/{year}: {monthly:N0} VNĐ");
                    break;

                case "3":
                    Console.Write("Nhập năm: ");
                    int y = int.Parse(Console.ReadLine());

                    double yearly = revenueService.GetRevenueByYear(y);

                    Console.WriteLine($"Doanh thu năm {y}: {yearly:N0} VNĐ");
                    break;

                case "4":
                    Console.Write("Nhập ngày bắt đầu: ");
                    DateTime start = DateTime.Parse(Console.ReadLine());

                    Console.Write("Nhập ngày kết thúc: ");
                    DateTime end = DateTime.Parse(Console.ReadLine());

                    double range = revenueService.GetRevenueByRange(start, end);

                    Console.WriteLine($"Doanh thu từ {start:dd/MM} đến {end:dd/MM}: {range:N0} VNĐ");
                    break;
            }

            Console.ReadKey();
        }
    }
}