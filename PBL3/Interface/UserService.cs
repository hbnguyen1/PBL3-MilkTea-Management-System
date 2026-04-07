using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;
using PBL3.Data;

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
    }
}
