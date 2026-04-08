using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;

namespace PBL3.Interface
{
    internal interface IOrderService
    {
        int CreateOrder(Orders order, List<OrderDetails> listorders);
    }
}
