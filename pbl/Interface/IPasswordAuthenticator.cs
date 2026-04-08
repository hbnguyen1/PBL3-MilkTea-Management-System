using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    internal interface IPasswordAuthenticator
    {
        Users? Authenticate(string phoneNumber, string password);
    }
}
