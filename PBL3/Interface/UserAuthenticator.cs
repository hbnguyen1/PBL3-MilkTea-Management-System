using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    internal class UserAuthenticator : IPasswordAuthenticator
    {
        public Users? Authenticate(string phoneNumber, string password)
        {
            // Mở kết nối Database
            using (var conn = new MilkTeaDBContext())
            {
                var loggedInUser = conn.Users.SingleOrDefault(u => u.Phone == phoneNumber && u.Password == password);
                return loggedInUser;
            }
        }
    }
}
