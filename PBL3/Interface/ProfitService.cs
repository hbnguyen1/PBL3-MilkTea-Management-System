using PBL3.Data;
using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;

namespace PBL3.Interface
{
    internal class ProfitService : IProfitService
    {
        // Lớp này sẽ sử dụng RevenueService để lấy doanh thu và sau đó tính toán lợi nhuận
        private readonly IRevenueService _revenueService;

        // Constructor để inject IRevenueService (Depency Injection)
        public ProfitService(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        public double GetGrossProfit(DateTime startDate, DateTime endDate)
        {
            // Bước 1: Gọi ông RevenueService ra tính tổng thu
            double totalRevenue = _revenueService.GetRevenueByRange(startDate, endDate);

            // Bước 2: Tự viết hàm tính tổng Giá vốn nguyên liệu đã tiêu hao
            double totalCostOfIngredients = CalculateTotalIngredientCost(startDate, endDate);

            // Bước 3: Lợi nhuận gộp = Thu - Chi
            return totalRevenue - totalCostOfIngredients;
        }

        //Định nghĩa interface IProfitService

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
        public double CalculateTotalSalaryCost(int month, int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                return conn.SalarySummaries.Where(e => e.month == month && e.year == year)
                                    .Sum(e => e.totalSalary);
            }
        }
        public ProfitReportDTO GetProfitReport(DateTime start, DateTime end)
        {
            double totalRevenue = _revenueService.GetRevenueByRange(start, end);
            double totalIngredientCost = CalculateTotalIngredientCost(start, end);
            double grossProfit = totalRevenue - totalIngredientCost; // Lợi nhuận gộp
            // Giả sử chúng ta tính chi phí lương theo tháng, lấy tháng và năm từ ngày bắt đầu
            int month = start.Month;
            int year = start.Year;
            double totalSalaryCost = CalculateTotalSalaryCost(month, year);
            return new ProfitReportDTO
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = totalRevenue,
                TotalIngredientCost = totalIngredientCost,
                TotalSalaryCost = totalSalaryCost
            };
        }
    }
}
