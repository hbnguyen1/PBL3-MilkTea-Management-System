using PBL3.Core;
using PBL3.Data;
using PBL3.Models;
using System;

namespace PBL3.Interface
{
    internal class CustomerPointService : ICustomerPointService
    {
        private const int MONEY_PER_POINT = 1000; // 1000 VND = 1 điểm

        public bool AddPoints(int customerId, double totalbill)
        {
            if (customerId == 1)
            {
                Logger.Info($"Khách hàng ID {customerId} là khách vãng lai, không cộng điểm.");
                return true;
            }

            int points = (int)totalbill / MONEY_PER_POINT;

            if (points <= 0)
            {
                Logger.Info($"Hóa đơn {totalbill} không đủ để cộng điểm (ID: {customerId}).");
                return false;
            }

            using (var conn = new MilkTeaDBContext())
            {
                var customer = conn.Customers.Find(customerId);

                if (customer == null)
                {
                    Logger.Error($"Không tìm thấy khách hàng ID: {customerId}.");
                    return false;
                }

                int oldPoints = customer.point;
                customer.point += points;

                conn.SaveChanges();

                CheckAndNotifyRankUp(oldPoints, customer.point);

                Logger.Info($"Đã cộng {points} điểm cho khách hàng ID: {customerId}.");
                return true;
            }
        }

        public int GetCurrentPoints(int customerId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var customer = conn.Customers.Find(customerId);
                return customer?.point ?? 0;
            }
        }

        public int GetCurrentPoints(Customer customer)
        {
            return customer?.point ?? 0;
        }

        public string GetCustomerRank(Customer customer)
        {
            if (customer == null)
                return "Uống nữa đi sắp lên hạng rồi";

            if (customer.userID == 1)
                return "Khách vãng lai";

            if (customer.point >= 300) return "Vàng";
            if (customer.point >= 200) return "Bạc";
            if (customer.point >= 100) return "Đồng";

            return "Uống nữa đi sắp lên hạng rồi";
        }
        public string GetCustomerRank(int customerId)
        {
            if (customerId == 1)
                return "Khách vãng lai";

            using (var conn = new MilkTeaDBContext())
            {
                var customer = conn.Customers.Find(customerId);
                return GetCustomerRank(customer); 
            }
        }
        public double GetDiscountPercentage(Customer customer)
        {
            if (customer == null || customer.userID == 1) return 0;

            if (customer.point >= 300) return 0.15;
            if (customer.point >= 200) return 0.1;
            if (customer.point >= 100) return 0.05;

            return 0;
        }

        public double GetDiscountPercentage(int customerId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var customer = conn.Customers.Find(customerId);
                return GetDiscountPercentage(customer); 
            }
        }

        private void CheckAndNotifyRankUp(int oldPoints, int newPoints)
        {
            if (oldPoints < 100 && newPoints >= 100)
                Console.WriteLine("\nCHÚC MỪNG! Lên hạng Đồng (Giảm 5%)!");

            else if (oldPoints < 200 && newPoints >= 200)
                Console.WriteLine("\nCHÚC MỪNG! Lên hạng Bạc (Giảm 10%)!");

            else if (oldPoints < 300 && newPoints >= 300)
                Console.WriteLine("\nCHÚC MỪNG! Lên hạng Vàng (Giảm 15%)!");
        }
    }
}