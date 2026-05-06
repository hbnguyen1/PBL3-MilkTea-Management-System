using PBL3.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    internal class RevenueService : IRevenueService
    {
        public double GetDailyRevenue(DateTime date)
        {
            using (var conn = new MilkTeaDBContext()){ 
                return conn.Orders.Where(o => o.orderStatus == "Completed"
                                         && o.orderDate.Date == date.Date)
                                  .Sum(o => o.totalPrice);

            }
        }
        public double GetRevenueByRange(DateTime start, DateTime end)
        {
            DateTime endOfDay = end.Date.AddDays(1).AddTicks(-1);
            using (var conn = new MilkTeaDBContext())
            {
                return conn.Orders.Where(o => o.orderStatus == "Completed"
                                         && o.orderDate >= start.Date
                                         && o.orderDate <= endOfDay)
                                  .Sum(o => o.totalPrice);
            }
        }
        public double GetRevenueByMonth(int month, int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                return conn.Orders.Where(o => o.orderStatus == "Completed"
                                         && o.orderDate.Month == month
                                         && o.orderDate.Year == year)
                                  .Sum(o => o.totalPrice);
            }

        }
        public double GetRevenueByYear(int year)
        {
            using (var conn = new MilkTeaDBContext())
            {
                return conn.Orders.Where(o => o.orderStatus == "Completed"
                                         && o.orderDate.Year == year)
                                  .Sum(o => o.totalPrice);
            }
        }
    }
}
