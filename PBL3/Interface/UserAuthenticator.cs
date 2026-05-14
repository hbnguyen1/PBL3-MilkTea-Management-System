using BCrypt.Net;
using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PBL3.Interface
{
    internal class UserAuthenticator : IPasswordAuthenticator
    {
        public Users? Authenticate(string phoneNumber, string password)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var loginUser = conn.Users.FirstOrDefault(u => u.Phone.Trim() == phoneNumber.Trim());
                if (loginUser != null)
                {
                    bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, loginUser.Password);
                    if (isPasswordCorrect)
                    {
                        return loginUser;
                    }
                }
                return null;
            }
        }
    }
}
