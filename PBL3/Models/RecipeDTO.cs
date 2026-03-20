using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Models
{
    public class RecipeDTO
    {
        public int itemID { get; set; }
        public string itemName { get; set; }
        public string size { get; set; }
        public string igName { get; set; }
        public int quantityNeeded { get; set; }
        public string unitUsed { get; set; }
    }
}
