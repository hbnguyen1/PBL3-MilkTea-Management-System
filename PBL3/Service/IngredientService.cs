using PBL3.Data;
using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;
using System.Text.RegularExpressions;
using PBL3.Core;
using PBL3.Interface;

namespace PBL3.Service
{
    internal class IngredientService : IIngredientService
    {
        private readonly MilkTeaDBContext _conn;
        public IngredientService(MilkTeaDBContext conn)
        {
            _conn = conn;
        }

        public bool AddIngredient(Ingredient ig)
            {
                    _conn.Ingredients.Add(ig);
                    _conn.SaveChanges();
                    return true;
            }
    
            public bool DeductStock(int igId, int quantity)
            {
                    var ig = _conn.Ingredients.Find(igId);
                    if (ig != null && ig.igCount >= quantity)
                    {
                        ig.igCount -= quantity;
                        _conn.SaveChanges();
                        return true;
                    }
                    return false;
                
            }
    
            public List<Ingredient> GetAllIngredients()
            {
                    var list = _conn.Ingredients.ToList();
                    return list;
            }

            public bool ImportStock(int staffId, List<ImportDetail> listAdd)
            {
                    var importNote = new ImportNote
                    {
                        staffID = staffId,
                        importDate = DateTime.Now,
                        ImportDetails = listAdd,
                        totalCost = listAdd.Sum(x => x.quantityAdded * x.importPrice)
                    };
                    _conn.ImportNotes.Add(importNote);
                    foreach (var item in listAdd)
                    {
                        var ig = _conn.Ingredients.Find(item.igId);
                        if (ig != null)
                        {
                            ig.igCount += item.quantityAdded;
                        }
                    }
                    _conn.SaveChanges();
                    return true;
                
            }
            
    
            public bool isAvailable(int igId, int requiredQuantity)
            {
                    var ig = _conn.Ingredients.Find(igId);
                    if (ig != null && ig.igCount >= requiredQuantity)
                    {
                        return true;
                    }
                    return false;
            }
            public List<Ingredient> GetLowStockIngredients()
            {
                    return _conn.Ingredients.Where(ig => ig.igCount < 500).ToList();
            }

        public bool updateIngredient(int igId, string name, string unit, int price)
        {
                var ig = _conn.Ingredients.Find(igId);
                if (ig != null)
                {
                    ig.igName = name;
                    ig.unit = unit;
                    ig.price = price;

                    _conn.SaveChanges();
                    return true;
                }
                return false;
        }
        public bool CheckIngredientEnough(int itemId, string size, int quantity)
            {
                    var recipes = _conn.Recipes
                        .Where(r => r.itemID == itemId && r.size == size)
                        .ToList();

                    foreach (var r in recipes)
                    {
                        var ingredient = _conn.Ingredients
                            .FirstOrDefault(i => i.igID == r.ingredientID);

                        if (ingredient == null)
                            return false;

                    int need = (int)(r.quantityNeeded * quantity);

                    if (ingredient.igCount < need)
                        {
                            Console.WriteLine($"Không đủ {ingredient.igName}");
                            Logger.Warning($"Ingredient low: {ingredient.igName} remain {ingredient.igCount}");
                            return false;
                        }
                    }

                    return true;
                }       
    }
}
