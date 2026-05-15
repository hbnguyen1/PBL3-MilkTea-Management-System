using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    public interface IUserService
    {
        Users? GetUserByPhone(string phone);
    }
}
