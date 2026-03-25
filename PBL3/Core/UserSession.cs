using PBL3.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;

namespace PBL3.Core
{
    public static class UserSession
    {
        public static Users? CurrentUser { get; set; }
    }
}
