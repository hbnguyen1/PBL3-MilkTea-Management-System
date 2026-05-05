using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Core;

namespace PBL3.Interface
{
    internal class RecipeService : IRecipeService
    {
        public bool AddRecipe(List<Recipe> lRecipe)
        {
            using (var db = new MilkTeaDBContext())
            {
                try
                {
                    db.Recipes.AddRange(lRecipe);
                    db.SaveChanges();

                    Logger.Info($"Đã thêm thành công {lRecipe.Count} dòng công thức mới.");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error("Lỗi khi thêm công thức: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        Logger.Error("Chi tiết SQL: " + ex.InnerException.Message);
                    }
                    return false;
                }
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
        public List<RecipeDTO>? GetAllRecipe()
        {
            using (var conn = new MilkTeaDBContext())
            {
                var listrecipe = conn.Recipes
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
