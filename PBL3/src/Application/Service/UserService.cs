using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualBasic.ApplicationServices;
using PBL3.src.Domain.Models;
using PBL3.src.Application.Interface;
using PBL3.src.Infrastructure.Data;

namespace PBL3.src.Application.Service
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
