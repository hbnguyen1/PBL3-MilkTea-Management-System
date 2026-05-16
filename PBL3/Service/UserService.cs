using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;
using PBL3.Data;
using PBL3.Interface;
using Microsoft.VisualBasic.ApplicationServices;

namespace PBL3.Service
{
    internal class UserService : IUserService
    {
        private readonly MilkTeaDBContext _conn;
        public UserService(MilkTeaDBContext conn)
        {
            _conn = conn;
        }
        public Users? GetUserByPhone(string phone)
        {
                var user = _conn.Users.FirstOrDefault(u => u.Phone == phone);
                return user;
        }
        public Users? GetUserById(int id)
        {
            var user = _conn.Users.FirstOrDefault(u => u.userID == id);
            return user;
        }
    }
}
