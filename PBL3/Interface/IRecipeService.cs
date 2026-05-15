using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;

namespace PBL3.Interface
{
    public interface IRecipeService
    {
        bool AddRecipe(List<Recipe> lRecipe);
        bool DeleteRecipeByID(int id);
        bool RemoveRecipeByObject(Recipe recipe);
        bool UpdateRecipe(Recipe recipe);
        Recipe? GetRecipebyID(int id);
        List<RecipeDTO>? GetAllRecipe();


    }
}
