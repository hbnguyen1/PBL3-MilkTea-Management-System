using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    internal interface IProfitService
    {
        ProfitReportDTO GetProfitReport(DateTime start, DateTime end);
        int CalculateTotalIngredientCost(DateTime start, DateTime end);
        double CalculateTotalSalaryCost(int month, int year);
    }
}
