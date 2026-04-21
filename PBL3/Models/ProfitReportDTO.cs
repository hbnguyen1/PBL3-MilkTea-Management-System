using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class ProfitReportDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double TotalRevenue { get; set; }

        public double TotalIngredientCost { get; set; }

        public double GrossProfit => TotalRevenue - TotalIngredientCost;

        public double TotalSalaryCost { get; set; }

        public double NetProfit => GrossProfit - TotalSalaryCost;
    }
}
