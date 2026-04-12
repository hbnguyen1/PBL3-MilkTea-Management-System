using PBL3.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{   
    public class Users 
    {
        public int userID { get; set; } = 0;//Khóa chính
        public required string Name { get; set; } = string.Empty;
        public required string Phone { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;   

        public Users() { }
    }
}
