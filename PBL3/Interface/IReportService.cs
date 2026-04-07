using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PBL3.Interface
{
    internal interface IReportService
    {
        public dynamic GetTopSellingItems(int top = 5);
    }
}
