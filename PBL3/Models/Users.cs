using PBL3.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PBL3.Models
{   
    public class Users 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int userID { get; set; } 
        public required string Name { get; set; } = string.Empty;
        public required string Phone { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;   

        public Users() { }
    }
}
