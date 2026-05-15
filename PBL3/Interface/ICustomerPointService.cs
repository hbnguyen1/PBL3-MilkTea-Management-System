using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    public interface ICustomerPointService
    {
        bool AddPoints(int customerId, int totalbill);
        int GetCurrentPoints(int customerId);
        string GetCustomerRank(int customerId);
        double GetDiscountPercentage(int customerId);
    }
}