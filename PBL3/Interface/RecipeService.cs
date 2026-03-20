using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    internal class RecipeService : IRecipeService
    {
        public bool AddRecipe(Recipe recipe)
        {
            using (var conn = new MilkTeaDBContext())
            {
                conn.Recipes.Add(recipe);
                conn.SaveChanges();
                return true;
            }
        }
        public bool DeleteRecipeByID(int id)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var recipe = conn.Recipes.Find(id);
                if (recipe != null)
                {
                    conn.Recipes.Remove(recipe);
                    conn.SaveChanges();
                    return true;
                }
            }
            return false;
        }
        public bool RemoveRecipeByObject(Recipe recipe)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var existingRecipe = conn.Recipes.Find(recipe.recipeID);
                if (existingRecipe != null)
                {
                    conn.Recipes.Remove(existingRecipe);
                    conn.SaveChanges();
                    return true;
                }
            }
            return false;
        }
        public bool UpdateRecipe(Recipe recipe)
        {
            throw new NotImplementedException();
        }
        public Recipe? GetRecipebyID(int id)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var recipe = conn.Recipes.Find(id);
                return recipe;
            }
        }
        public List<RecipeDTO>? GetAllRecipe() {
            using (var conn = new MilkTeaDBContext())
            {
                var listrecipe = conn.Recipes
                    .Include(r => r.Item)
                    .Include(r => r.Ingredient)
                    .Select(r => new RecipeDTO
                    {
                        itemID = r.itemID,
                        itemName = r.Item.itemName,       
                        size = r.size,
                        igName = r.Ingredient.igName, 
                        quantityNeeded = r.quantityNeeded,         
                        unitUsed = r.unitUsed                  
                    })
                    .ToList();
                return listrecipe;
            }
        }
    }
}
