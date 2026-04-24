using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using System;
using System.Linq;

namespace PBL3.Interface
{
    internal class ProfitService : IProfitService
    {
        // Chi phí mặt bằng cố định hàng tháng
        const double premisesFee = 10000000;

        // 1. HÀM LÕI: Tính Lợi nhuận gộp (Bán hàng - Tiền nguyên liệu) siêu tốc
        public double GetProfitInRange(DateTime startDate, DateTime endDate)
        {
            using (var conn = new MilkTeaDBContext())
            {
                DateTime endOfDay = endDate.Date.AddDays(1).AddTicks(-1);

                return conn.OrderDetails
                    .Include(od => od.Order)
                    .Where(od => od.Order != null
                              && od.Order.orderStatus.Trim().ToLower() == "approved"
                              && od.Order.orderDate >= startDate.Date
                              && od.Order.orderDate <= endOfDay)
                    .Sum(od => od.quantity * (od.priceAtOrder - od.costAtOrder ?? 0));
            }
        }

        // Theo Ngày (Chỉ xem Lợi nhuận gộp, vì chia nhỏ tiền mặt bằng theo ngày rất vô lý)
        public double GetDayProfit(DateTime day)
        {
            return GetProfitInRange(day, day);
        }

        // Theo Tháng (Lợi nhuận gộp - Lương - Mặt bằng)
        public double GetProfitByMonth(int month, int year)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1).AddTicks(-1);

            double grossProfit = GetProfitInRange(start, end);
            double salaryCost = CalculateTotalSalaryCost(month, year);

            // Lợi nhuận ròng tháng = Gộp - Lương - (1 tháng mặt bằng)
            return grossProfit - salaryCost - premisesFee;
        }

        // Theo Quý (Lợi nhuận gộp - Lương 3 tháng - Mặt bằng 3 tháng)
        public double GetProfitByQuarter(int quarter, int year)
        {
            DateTime start;
            DateTime end;
            switch (quarter)
            {
                case 1:
                    start = new DateTime(year, 1, 1);
                    end = new DateTime(year, 3, 31).AddDays(1).AddTicks(-1);
                    break;
                case 2:
                    start = new DateTime(year, 4, 1);
                    end = new DateTime(year, 6, 30).AddDays(1).AddTicks(-1);
                    break;
                case 3:
                    start = new DateTime(year, 7, 1);
                    end = new DateTime(year, 9, 30).AddDays(1).AddTicks(-1);
                    break;
                case 4:
                    start = new DateTime(year, 10, 1);
                    end = new DateTime(year, 12, 31).AddDays(1).AddTicks(-1);
                    break;
                default:
                    throw new ArgumentException("Quý không hợp lệ. Vui lòng nhập giá trị từ 1 đến 4.");
            }

            double grossProfit = GetProfitInRange(start, end);
            double salaryCost = CalculateQuarterTotalSalaryCost(quarter, year);

            // Lợi nhuận ròng quý = Gộp - Lương Quý - (3 tháng mặt bằng)
            return grossProfit - salaryCost - (premisesFee * 3);
        }

        // Theo Năm (Lợi nhuận gộp - Lương 12 tháng - Mặt bằng 12 tháng)
        public double GetProfitByYear(int year)
        {
            DateTime start = new DateTime(year, 1, 1);
            DateTime end = new DateTime(year, 12, 31).AddDays(1).AddTicks(-1);

            double grossProfit = GetProfitInRange(start, end);
            double salaryCost = CalculateYearTotalSalaryCost(year);

            // Lợi nhuận ròng năm = Gộp - Lương Năm - (12 tháng mặt bằng)
            return grossProfit - salaryCost - (premisesFee * 12);
        }

        public double CalculateTotalSalaryCost(int month, int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                // Dùng ?? 0.0 để phòng trường hợp tháng đó không có bảng lương nào, tránh bị lỗi Null
                return conn.SalarySummaries
                           .Where(e => e.month == month && e.year == year)
                           .Sum(e => (double?)e.totalSalary) ?? 0.0;
            }
        }

        public double CalculateQuarterTotalSalaryCost(int quarter, int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                int startMonth = (quarter - 1) * 3 + 1;
                int endMonth = startMonth + 2;
                return conn.SalarySummaries
                           .Where(e => e.year == year && e.month >= startMonth && e.month <= endMonth)
                           .Sum(e => (double?)e.totalSalary) ?? 0.0;
            }
        }

        public double CalculateYearTotalSalaryCost(int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                return conn.SalarySummaries
                           .Where(e => e.year == year)
                           .Sum(e => (double?)e.totalSalary) ?? 0.0;
            }
        }
        public int CalculateTotalIngredientCost(DateTime start, DateTime end)
        {
            DateTime endOfDay = end.Date.AddDays(1).AddTicks(-1);
            using (var conn = new MilkTeaDBContext())
            {
                var totalcost = from o in conn.Orders
                                //Lọc các đơn hàng đã hoàn thành trong khoảng thời gian được chọn
                                where o.orderStatus == "Approved"
                                      && o.orderDate >= start.Date
                                      && o.orderDate <= endOfDay

                                //Kết nối bảng Order và OrderDetails bằng orderID để lấy chi tiết từng đơn hàng
                                join od in conn.OrderDetails on o.orderID equals od.orderID

                                //Kết nối bảng OrderDetails với bảng Recipes để lấy thông tin về công thức và số lượng nguyên liệu cần cho từng đơn
                                join r in conn.Recipes on new
                                {
                                    id = od.itemID,
                                    size = od.size
                                }
                                equals new
                                {
                                    id = r.itemID,
                                    size = r.size
                                }

                                //Kết nối bảng Recipes với bảng Ingredients để lấy giá của từng nguyên liệu
                                join ig in conn.Ingredients on r.ingredientID equals ig.igID

                                select new
                                {
                                    TotalCost = od.quantity * r.quantityNeeded * ig.price
                                };
                return (int)totalcost.Sum(x => x.TotalCost);
            }
        }
    }
}