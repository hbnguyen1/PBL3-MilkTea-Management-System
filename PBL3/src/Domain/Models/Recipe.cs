using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.src.Domain.Models
{
    public class Recipe
    {
        public int recipeID { get; set; }

        public string size { get; set; } = string.Empty;

        public int itemID { get; set; }

        public int ingredientID { get; set; }

        public int quantityNeeded { get; set; }

        public string unitUsed { get; set; } = string.Empty;

        public virtual Item Item { get; set; } = null!;

        public virtual Ingredient Ingredient { get; set; } = null!;
    }
}