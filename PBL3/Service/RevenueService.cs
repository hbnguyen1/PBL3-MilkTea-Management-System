using PBL3.Data;
using PBL3.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PBL3.Service
{
    internal class RevenueService : IRevenueService
    {
        private readonly MilkTeaDBContext _conn;

        public RevenueService(MilkTeaDBContext conn)
        {
            _conn = conn;
        }

        public double GetDailyRevenue(DateTime date)
        {
            return _conn.Orders.Where(o => o.orderStatus == "Completed"
                                        && o.orderDate.Date == date.Date)
                               .Sum(o => o.totalPrice);
        }

        public double GetRevenueByRange(DateTime start, DateTime end)
        {
            DateTime endOfDay = end.Date.AddDays(1).AddTicks(-1);

            return _conn.Orders.Where(o => o.orderStatus == "Completed"
                                        && o.orderDate >= start.Date
                                        && o.orderDate <= endOfDay)
                               .Sum(o => o.totalPrice);
        }

        public double GetRevenueByMonth(int month, int year)
        {
            return _conn.Orders.Where(o => o.orderStatus == "Completed"
                                        && o.orderDate.Month == month
                                        && o.orderDate.Year == year)
                               .Sum(o => o.totalPrice);
        }

        public double GetRevenueByYear(int year)
        {
            return _conn.Orders.Where(o => o.orderStatus == "Completed"
                                        && o.orderDate.Year == year)
                               .Sum(o => o.totalPrice);
        }
    }
}