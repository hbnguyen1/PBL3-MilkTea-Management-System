using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;

namespace PBL3.Interface
{
    public interface IIngredientService
    {
        bool AddIngredient(Ingredient ig);
        bool updateIngredient(int igId, string name, string unit, int price);
        bool ImportStock(int staffId, List<ImportDetail> listAdd);
        bool isAvailable(int igId, int requiredQuantity);
        bool DeductStock(int igId, int quantity);
        List<Ingredient> GetAllIngredients();
        public List<Ingredient> GetLowStockIngredients();
        bool CheckIngredientEnough(int itemId, string size, int quantity);
    }
}
