using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PBL3.Core;
using PBL3.Interface;

namespace PBL3.Service
{
    internal class RecipeService : IRecipeService
    {
        private readonly MilkTeaDBContext _conn;
        public RecipeService(MilkTeaDBContext conn)
        {
            _conn = conn;
        }

        public bool AddRecipe(List<Recipe> lRecipe)
        {
            try
            {
                _conn.Recipes.AddRange(lRecipe);
                _conn.SaveChanges();

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

        public bool DeleteRecipeByID(int id)
        {
            var recipe = _conn.Recipes.Find(id);
            if (recipe != null)
            {
                _conn.Recipes.Remove(recipe);
                _conn.SaveChanges();
                return true;
            }
            return false;
        }

        public bool RemoveRecipeByObject(Recipe recipe)
        {
            var existingRecipe = _conn.Recipes.Find(recipe.recipeID);
            if (existingRecipe != null)
            {
                _conn.Recipes.Remove(existingRecipe);
                _conn.SaveChanges();
                return true;
            }
            return false;
        }

        public bool UpdateRecipe(Recipe recipe)
        {
            throw new NotImplementedException();
        }

        public Recipe? GetRecipebyID(int id)
        {
            return _conn.Recipes.Find(id);
        }

        public List<RecipeDTO>? GetAllRecipe()
        {
            var listrecipe = _conn.Recipes
                .Include(r => r.Item)
                .Include(r => r.Ingredient)
                .Select(r => new RecipeDTO
                {
                    itemID = r.itemID,
                    itemName = r.Item.itemName,
                    size = r.size,
                    igName = r.Ingredient.igName,
                    quantityNeeded = (int)r.quantityNeeded,
                    unitUsed = r.unitUsed
                })
                .ToList();
            return listrecipe;
        }
    }
}