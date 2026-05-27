using PBL3.src.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.src.Application.Interface
{
    public interface IPasswordAuthenticator
    {
        Users? Authenticate(string phoneNumber, string password);
    }
}
