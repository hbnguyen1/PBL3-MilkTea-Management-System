using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    internal interface IProfitService
    {
        int CalculateTotalIngredientCost(DateTime start, DateTime end);
        double CalculateTotalSalaryCost(int month, int year);
        double GetProfitInRange(DateTime startDate, DateTime endDate);
        double GetDayProfit(DateTime day);
        double GetProfitByMonth(int month, int year);
        double GetProfitByQuarter(int quarter, int year);
        double GetProfitByYear(int year);
        double CalculateYearTotalSalaryCost(int year);
        double CalculateQuarterTotalSalaryCost(int quarter, int year);
    }
}
