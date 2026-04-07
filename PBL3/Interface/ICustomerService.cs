using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    public interface ICustomerService
    {
        bool AddNewCustomer(string name, string phoneNumber, string password);
        public List<string> GetTrendingItemNamesForCustomer();
  
    }
}
