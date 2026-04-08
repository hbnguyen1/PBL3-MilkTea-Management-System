using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PBL3.Models
{
    public class Recipe
    {
        public int recipeID { get; set; } = 0;
        public required string size { get; set; } = string.Empty;
        public required int itemID { get; set; } = 0;
        public required int ingredientID { get; set; } = 0;
        public required double quantityNeeded { get; set; } = 0;
        public required string unitUsed { get; set; } = string.Empty;
        //public virtual Ingredient? IngredientInfo { get; set; }
        //public virtual Item? ItemInfo { get; set; }
        //[ForeignKey("itemID")]
        public virtual Item Item { get; set; }
        //[ForeignKey("igID")]
        public virtual Ingredient Ingredient { get; set; }
    }
}
