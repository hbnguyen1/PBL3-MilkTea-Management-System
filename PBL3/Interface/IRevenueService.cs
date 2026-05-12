using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PBL3.Interface
{
    internal interface IRevenueService
    {
        double GetDailyRevenue(DateTime date);
        double GetRevenueByRange(DateTime start, DateTime end);
        double GetRevenueByMonth(int month, int year);
        double GetRevenueByYear(int year);
    }
}
