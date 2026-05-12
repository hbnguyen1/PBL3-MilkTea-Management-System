using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using System;
using System.Linq;

namespace PBL3.Interface
{
    public class ProfitService : IProfitService
    {
        const double premisesFee = 10000000;

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
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1).AddTicks(-1);

            double grossProfit = GetProfitInRange(start, end);
            double salaryCost = CalculateTotalSalaryCost(month, year);

            return grossProfit - salaryCost - premisesFee;
        }

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
                    throw new ArgumentException("Quý không hợp lệ (1-4)");
            }

            double grossProfit = GetProfitInRange(start, end);
            double salaryCost = CalculateQuarterTotalSalaryCost(quarter, year);

            return grossProfit - salaryCost - (premisesFee * 3);
        }

        public double GetProfitByYear(int year)
        {
            DateTime start = new DateTime(year, 1, 1);
            DateTime end = new DateTime(year, 12, 31).AddDays(1).AddTicks(-1);

            double grossProfit = GetProfitInRange(start, end);
            double salaryCost = CalculateYearTotalSalaryCost(year);

            return grossProfit - salaryCost - (premisesFee * 12);
        }

        public double CalculateTotalSalaryCost(int month, int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                // ĐÃ SỬA: Thêm chữ 's' thành SalarySummaries cho đúng với Database
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

                // ĐÃ SỬA: Thêm chữ 's' thành SalarySummaries
                return conn.SalarySummaries
                    .Where(e => e.year == year && e.month >= startMonth && e.month <= endMonth)
                    .Sum(e => (double?)e.totalSalary) ?? 0.0;
            }
        }

        public double CalculateYearTotalSalaryCost(int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                // ĐÃ SỬA: Thêm chữ 's' thành SalarySummaries
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

                                join ig in conn.Ingredients on r.ingredientID equals ig.igID

                                select od.quantity * r.quantityNeeded * ig.price;

                return (int)totalcost.Sum();
            }
        }
    }
}