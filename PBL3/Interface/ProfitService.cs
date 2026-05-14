using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using System;
using System.Linq;

namespace PBL3.Interface
{
    public class ProfitService : IProfitService
    {
        const double premisesFee = 3000000;

        public double GetProfitInRange(DateTime startDate, DateTime endDate)
        {
            using (var conn = new MilkTeaDBContext())
            {
                DateTime endOfDay = endDate.Date.AddDays(1).AddTicks(-1);

                var profit = (from o in conn.Orders
                              join od in conn.OrderDetails on o.orderID equals od.orderID
                              where o.orderStatus == "Completed"
                                 && o.orderDate >= startDate.Date
                                 && o.orderDate <= endOfDay
                              select od.quantity * (od.priceAtOrder - (od.costAtOrder ?? 0.0))).Sum();

                return profit;
            }
        }

        public double GetDayProfit(DateTime day)
        {
            return GetProfitInRange(day, day);
        }

        public double GetProfitByMonth(int month, int year)
        {
            DateTime currentDate = DateTime.Now;
            if (year > currentDate.Year || (year == currentDate.Year && month > currentDate.Month))
            {
                return 0;
            }

            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1).AddTicks(-1);

            double doanhThu = 0;
            using (var conn = new MilkTeaDBContext())
            {
                doanhThu = conn.Orders.Where(o => o.orderStatus == "Completed"
                                               && o.orderDate.Month == month
                                               && o.orderDate.Year == year)
                                      .Sum(o => o.totalPrice);
            }

            double chiPhiNguyenLieu = CalculateTotalIngredientCost(start, end);
            double salaryCost = CalculateTotalSalaryCost(month, year);

            return doanhThu - chiPhiNguyenLieu - salaryCost - premisesFee;
        }

        public double GetProfitByQuarter(int quarter, int year)
        {
            int currentQuarter = (DateTime.Now.Month - 1) / 3 + 1;
            if (year > DateTime.Now.Year || (year == DateTime.Now.Year && quarter > currentQuarter))
            {
                return 0;
            }

            DateTime start = new DateTime(year, (quarter - 1) * 3 + 1, 1);
            DateTime end = start.AddMonths(3).AddDays(-1);

            double doanhThu = 0;
            using (var conn = new MilkTeaDBContext())
            {
                doanhThu = conn.Orders.Where(o => o.orderStatus == "Completed"
                                               && o.orderDate >= start.Date
                                               && o.orderDate <= end)
                                      .Sum(o => o.totalPrice);
            }

            double chiPhiNguyenLieu = CalculateTotalIngredientCost(start, end);
            double salaryCost = CalculateQuarterTotalSalaryCost(quarter, year);

            return doanhThu - chiPhiNguyenLieu - salaryCost - (premisesFee * 3);
        }

        public double GetProfitByYear(int year)
        {
            if (year > DateTime.Now.Year) return 0;

            DateTime start = new DateTime(year, 1, 1);
            DateTime end = new DateTime(year, 12, 31).AddDays(1).AddTicks(-1);

            double doanhThu = 0;
            using (var conn = new MilkTeaDBContext())
            {
                doanhThu = conn.Orders.Where(o => o.orderStatus == "Completed"
                                               && o.orderDate.Year == year)
                                      .Sum(o => o.totalPrice);
            }

            double chiPhiNguyenLieu = CalculateTotalIngredientCost(start, end);
            double salaryCost = CalculateYearTotalSalaryCost(year);

            return doanhThu - chiPhiNguyenLieu - salaryCost - (premisesFee * 12);
        }

        public double CalculateTotalSalaryCost(int month, int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
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
                                where o.orderStatus == "Completed"
                                      && o.orderDate >= start.Date
                                      && o.orderDate <= endOfDay

                                join od in conn.OrderDetails on o.orderID equals od.orderID
                                join r in conn.Recipes on new { id = od.itemID, size = od.size } equals new { id = r.itemID, size = r.size }
                                join ig in conn.Ingredients on r.ingredientID equals ig.igID

                                select od.quantity * r.quantityNeeded * ig.price;

                return (int)totalcost.Sum();
            }
        }
    }
}