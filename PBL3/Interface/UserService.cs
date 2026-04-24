using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    internal class UserService
    {
        public Users? GetUserByPhone(string phone)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var user = conn.Users.FirstOrDefault(u => u.Phone == phone);
                return user;
            }
        }
        public bool AddNewStaff(string name, string phone, string password, int salary, bool isAvailable)
        {
            using (var conn = new MilkTeaDBContext())
            {
                if (conn.Users.Any(u => u.Phone == phone))
                {
                    return false;
                }
                var newUser = new Staff
                {
                    Name = name,
                    Phone = phone,
                    Password = password,
                    salaryPerHour = salary
                };
                conn.Staffs.Add(newUser);
                conn.SaveChanges();
                return true;
            }
        }
        public List<(int userID, string name, string phoneNumber, double? salaryPerHour)> GetAllStaffs()
        {
            using (var conn = new MilkTeaDBContext())
            {
                // 1. Join và lấy dữ liệu bằng Anonymous Type (kiểu ẩn danh)
                var query = from s in conn.Staffs
                            join u in conn.Users on s.userID equals u.userID
                            select new 
                            { 
                                s.userID, 
                                u.Name, 
                                u.Phone, 
                                s.salaryPerHour 
                            };

                // 2. Ép nó về danh sách Tuple và trả về (AsEnumerable giúp tránh lỗi dịch của EF Core)
                return query.AsEnumerable()
                                        .Select(x => (
                                            userID: x.userID,
                                            name: x.Name,                           
                                            phoneNumber: x.Phone,                 
                                            salaryPerHour: (double?)x.salaryPerHour
                                        ))
                                        .ToList();
            }
        }
        public bool UpdateStaffisntAvailable(int staffId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var staff = conn.Staffs.FirstOrDefault(s => s.userID == staffId);
                if (staff == null)
                {
                    return false;
                }
                staff.isAvailable = false;
                conn.SaveChanges();
                return true;
            }
        }
    }
}
