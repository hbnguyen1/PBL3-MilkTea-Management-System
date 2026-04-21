using Microsoft.Identity.Client;
using PBL3.Core;
using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    internal class CustomerPointService : ICustomerPointService
    {
        private const int MONEY_PER_POINT = 1000; //1000vnd = 1d
        public bool AddPoints(int customerId, double totalbill)
        {
            if (customerId == 1)
            {
                Logger.Info($"Khách hàng có ID: {customerId} là khách vãng lai, không được cộng điểm.");
                return true;
            }
            int points = (int)totalbill / MONEY_PER_POINT;
            if (points > 0)
            {
                using (var conn = new MilkTeaDBContext())
                {
                    var customer = conn.Customers.Find(customerId);
                    if (customer != null)
                    {   
                        int oldPoints = customer.point;
                        customer.point = oldPoints + points;
                        conn.SaveChanges();
                        CheckAndNotifyRankUp(oldPoints, customer.point);
                        Logger.Info($"Thêm {points} điểm cho khách hàng có ID: {customerId}.");
                        return true;
                    }
                    else
                    {
                        Logger.Error($"Không tìm thấy khách hàng có ID: {customerId} để thêm điểm.");
                        return false;
                    }
                }
            }
            else
            {
                Logger.Info($"Tổng hóa đơn {totalbill} không đủ để cộng điểm cho khách hàng có ID: {customerId}.");
                return false;
            }
 

        }
        public int GetCurrentPoints(int customerId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var customer = conn.Customers.Find(customerId);
                if (customer != null)
                {
                    return customer.point;
                }
            }
            return 0;
        }
        public string GetCustomerRank(int customerId)
        {
            if (customerId == 1)
            {
                return "Khách vãng lai";
            }
            using (var conn = new MilkTeaDBContext())
            {
                var customer = conn.Customers.Find(customerId);
                if (customer != null)
                {

                    if (customer.point >= 300)
                    {
                        return "Vàng";
                    }
                    if (customer.point >= 200)
                    {
                        return "Bạc";
                    }
                    if (customer.point >= 100)
                    {
                        return "Đồng";
                    }
                }
            }
            return "Uống nữa đi sắp lên hạng rồi";
        }
        public double GetDiscountPercentage(int customerId)
        {
            string rank = GetCustomerRank(customerId);
            return rank switch
            {
                "Đồng" => 0.05,
                "Bạc" => 0.1,
                "Vàng" => 0.15,
                "Uống nữa đi sắp lên hạng rồi" => 0,
                "Khách vãng lai" => 0
            };
        }
        private void CheckAndNotifyRankUp(int oldPoints, int newPoints)
        {
            if (oldPoints < 100 && newPoints >= 100)
                Console.WriteLine("\nCHÚC MỪNG! Khách hàng đã thăng hạng Đồng (Giảm 5% cho lần sau)! Uống mạnh quá khách yêu ơi");
            else if (oldPoints < 200 && newPoints >= 200)
                Console.WriteLine("\nCHÚC MỪNG! Khách hàng đã thăng hạng Bạc (Giảm 10% cho lần sau)! Uống mạnh quá khách yêu ơi");
            else if (oldPoints < 300 && newPoints >= 300)
                Console.WriteLine("\nCHÚC MỪNG! Khách hàng đã thăng hạng Vàng (Giảm tối đa 15%)! Uống mạnh quá khách yêu ơi");
        }
    }
}
