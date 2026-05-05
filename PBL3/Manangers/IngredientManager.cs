using System;
using System.Collections.Generic;
using PBL3.Interface;
using PBL3.Models;

namespace PBL3.Manangers
{
    internal class IngredientManager
    {
        public bool AddIngredient(string name, int price, string unit, int count)
        {
            if (string.IsNullOrEmpty(name)) return false;

            IngredientService ingredientService = new IngredientService();

            Ingredient ig = new Ingredient()
            {
                igName = name,
                price = price,
                unit = unit,
                igCount = count
            };

            return ingredientService.AddIngredient(ig);
        }
        public bool UpdateIngredientInfo(int id, string name, int price, string unit)
        {
            if (string.IsNullOrEmpty(name)) return false;

            IngredientService ingredientService = new IngredientService();
            return ingredientService.updateIngredient(id, name, unit, price);
        }

        public List<Ingredient> GetAllIngredients()
        {
            IngredientService ingredientService = new IngredientService();
            return ingredientService.GetAllIngredients();
        }

        public List<Ingredient> GetLowStockIngredients()
        {
            IngredientService ingredientService = new IngredientService();
            return ingredientService.GetLowStockIngredients();
        }
    }
}