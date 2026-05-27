using BCrypt.Net;
using PBL3.src.Application.Interface;
using PBL3.src.Domain.Models;
using PBL3.src.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PBL3.src.Application.Service
{
    internal class UserAuthenticator : IPasswordAuthenticator
    {
        private readonly MilkTeaDBContext _conn;
        public UserAuthenticator(MilkTeaDBContext conn)
        {
            _conn = conn;
        }

        public Users? Authenticate(string phoneNumber, string password)
        {
            var loginUser = _conn.Users.FirstOrDefault(u => u.Phone.Trim() == phoneNumber.Trim());

            if (loginUser != null)
            {
                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, loginUser.Password.Trim());

                if (isPasswordCorrect)
                {
                    return loginUser;
                }
            }
            return null;
        }
    }
}