using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Interface;
using PBL3.Models;

namespace PBL3.Manangers
{
    internal class RecipeManager
    {
        public void ShowRecipe()
        {  
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            RecipeService recipeService = new RecipeService();
            var listRecipe = recipeService.GetAllRecipe();
            var GroupItem = listRecipe
                .GroupBy(r => new { r.itemID, r.itemName, r.size })
                .ToList();
            foreach (var group in GroupItem)
            {
                string itemName = group.Key.itemName;
                string size = group.Key.size;
                var iglist = group.Select(r => $"{r.igName} {r.quantityNeeded} {r.unitUsed}");
                string igit = string.Join(", ", iglist);
                Console.WriteLine($"Món: {itemName}, Size: {size}, Thành phần: {igit}");
            }
        }
    }
}
