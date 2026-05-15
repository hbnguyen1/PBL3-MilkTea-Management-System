using System;
using System.Linq;
using PBL3.Core;
using PBL3.Data;
using PBL3.Interface;
using PBL3.Models;

namespace PBL3.Service
{
    internal class CustomerPointService : ICustomerPointService
    {
        private readonly MilkTeaDBContext _conn;
        public CustomerPointService(MilkTeaDBContext conn)
        {
            _conn = conn;
        }
        private const int MONEY_PER_POINT = 1000;

        public bool AddPoints(int customerId, int totalbill)
        {
            if (customerId == 1) return false;

            int points = totalbill / MONEY_PER_POINT;
            if (points > 0)
            {
                    var customer = _conn.Customers.Find(customerId);
                    if (customer != null)
                    {
                        customer.point += points;
                        _conn.SaveChanges();
                        Logger.Info($"Thêm {points} điểm cho khách hàng có ID: {customerId}");
                        return true;
                    }
            }
            return false;
        }

        public int GetCurrentPoints(int customerId)
        {
                var customer = _conn.Customers.Find(customerId);
                return customer != null ? customer.point : 0;
        }

        public string GetCustomerRank(int customerId)
        {
            if (customerId == 1) return "Khách vãng lai";
                var customer = _conn.Customers.Find(customerId);
                if (customer != null)
                {
                    if (customer.point >= 300) return "Vàng";
                    if (customer.point >= 200) return "Bạc";
                    if (customer.point >= 100) return "Đồng";
                }
            return "Chưa có hạng";
        }

        public double GetDiscountPercentage(int customerId)
        {
            string rank = GetCustomerRank(customerId);
            return rank switch
            {
                "Đồng" => 0.05,
                "Bạc" => 0.10,
                "Vàng" => 0.15,
                _ => 0
            };
        }
    }
}