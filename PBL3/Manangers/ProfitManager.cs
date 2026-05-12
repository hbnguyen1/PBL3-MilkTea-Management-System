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
            ProfitService profitService = new ProfitService();
            Console.WriteLine("===== BÁO CÁO LỢI NHUẬN =====");
            Console.WriteLine("1. Lợi nhuận theo ngày");
            Console.WriteLine("2. Lợi nhuận theo tháng");
            Console.WriteLine("3. Lợi nhuận theo quý");
            Console.WriteLine("4. Lợi nhuận theo năm");
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    string inputDate = Console.ReadLine();
                    if (DateTime.TryParseExact(inputDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime day))
                    {
                        double profit = profitService.GetDayProfit(day);
                        Console.WriteLine($"Lợi nhuận ngày {day:dd/MM/yyyy}: {profit} VND");
                    }
                    else
                    {
                        Console.WriteLine("Ngày không hợp lệ!");
                    }
                    break;
                case "2":
                    if (DateTime.TryParseExact(Console.ReadLine(), "MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime monthYear))
                    {
                        double profit = profitService.GetProfitByMonth(monthYear.Month, monthYear.Year);
                        Console.WriteLine($"Lợi nhuận tháng {monthYear:MM/yyyy}: {profit} VND");
                    }
                    else
                    {
                        Console.WriteLine("Tháng không hợp lệ!");
                    }

                    break;
                case "3":
                    if (DateTime.TryParseExact(Console.ReadLine(), "Q/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime quarterYear))
                    {
                        int quarter = (quarterYear.Month - 1) / 3 + 1;
                        double profit = profitService.GetProfitByQuarter(quarter, quarterYear.Year);
                        Console.WriteLine($"Lợi nhuận quý {quarter}/{quarterYear:yyyy}: {profit} VND");
                    }
                    else
                    {
                        Console.WriteLine("Quý không hợp lệ!");
                    }
                    break;
                case "4":
                    if (int.TryParse(Console.ReadLine(), out int year))
                    {
                        double profit = profitService.GetProfitByYear(year);
                        Console.WriteLine($"Lợi nhuận năm {year}: {profit} VND");
                    }
                    else
                    {
                        Console.WriteLine("Năm không hợp lệ!");
                    }
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ!");
                    break;
            }
        }
    }
}
