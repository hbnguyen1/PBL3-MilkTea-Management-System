using System;

namespace PBL3.src.Domain.Models
{
    public class Customer : Users
    {
        public required int point { get; set; } = 0;
    }
}
