using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;
using PBL3.Core;
using PBL3.Data;
using Microsoft.IdentityModel.Tokens;

namespace PBL3.Interface
{
    internal class ReportService : IReportService
    {
        public dynamic GetTopSellingItems(int top = 5)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var result = (from od in conn.OrderDetails
                              join i in conn.Items on od.itemID equals i.itemID
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
        }

        /// <summary>
        /// Lấy danh sách các itemID của top 5 sản phẩm bán chạy nhất
        /// </summary>
        public List<int> GetBestSellerItemIDs(int top = 5)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var bestSellerIDs = (from od in conn.OrderDetails
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
}
