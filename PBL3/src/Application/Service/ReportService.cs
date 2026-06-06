using Microsoft.IdentityModel.Tokens;
using PBL3.src.Domain.Models;
using PBL3.src.Application.Interface;
using PBL3.src.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.src.Application.Service
{
    internal class ReportService : IReportService
    {
        private readonly MilkTeaDBContext _conn;
        public ReportService(MilkTeaDBContext conn)
        {
            _conn = conn;
        }
        public dynamic GetTopSellingItems(int top = 5)
        {
                var result = (from od in _conn.OrderDetails
                              join i in _conn.Items on od.itemID equals i.itemID
                              // Nhóm luôn cả ID, Tên và Giá lại với nhau để xíu nữa lấy cho dễ
                              group new { od, i } by new { i.itemID, i.itemName, i.price } into g
                              select new
                              {
                                  ItemID = g.Key.itemID,
                                  ItemName = g.Key.itemName,
                                  TotalQuantity = g.Sum(x => x.od.quantity),
                                  // Lúc này lấy Giá (price) nhân với Số lượng (quantity) cực kỳ an toàn
                                  TotalRevenue = g.Sum(x => x.od.quantity * g.Key.price)
                              })
                             .OrderByDescending(x => x.TotalQuantity)
                             .Take(top)
                             .ToList();
                return result;
            
        }
        public List<int> GetBestSellerItemIDs(int top = 5)
        {
                var bestSellerIDs = (from od in _conn.OrderDetails
                                     group od by od.itemID into g
                                     select new
                                     {
                                         ItemID = g.Key,
                                         TotalQuantity = g.Sum(x => x.quantity)
                                     })
                                    .OrderByDescending(x => x.TotalQuantity)
                                    .Take(top)
                                    .Select(x => x.ItemID)
                                    .ToList();
                return bestSellerIDs;
            }
    }
}
