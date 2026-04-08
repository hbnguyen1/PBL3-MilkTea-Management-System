using PBL3.Data;
using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Models;
using System.Text.RegularExpressions;

namespace PBL3.Interface
{
    internal class IngredientService : IIngredientService
    {
            public bool AddIngredient(Ingredient ig)
            {
                using (var conn = new MilkTeaDBContext()) 
                { 
                    conn.Ingredients.Add(ig);
                    conn.SaveChanges();
                    return true;
                }
            }
    
            public bool DeductStock(int igId, int quantity)
            {
                using (var conn = new MilkTeaDBContext())
                {
                    var ig = conn.Ingredients.Find(igId);
                    if (ig != null && ig.igCount >= quantity)
                    {
                        ig.igCount -= quantity;
                        conn.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
    
            public List<Ingredient> GetAllIngredients()
            {
                using (var conn = new MilkTeaDBContext())
                {
                    var list = conn.Ingredients.ToList();
                    return list;
                }
            }

            public bool ImportStock(int staffId, List<ImportDetail> listAdd)
            {
                using (var conn = new MilkTeaDBContext())
                {
                    var importNote = new ImportNote
                    {
                        staffID = staffId,
                        importDate = DateTime.Now,
                        ImportDetails = listAdd,
                        totalCost = listAdd.Sum(x => x.quantityAdded * x.importPrice)
                    };
                    conn.ImportNotes.Add(importNote);
                    foreach (var item in listAdd)
                    {
                        var ig = conn.Ingredients.Find(item.igId);
                        if (ig != null)
                        {
                            ig.igCount += item.quantityAdded;
                        }
                    }
                    conn.SaveChanges();
                    return true;
                }
            }
            
    
            public bool isAvailable(int igId, int requiredQuantity)
            {
                using (var conn = new MilkTeaDBContext())
                {
                    var ig = conn.Ingredients.Find(igId);
                    if (ig != null && ig.igCount >= requiredQuantity)
                    {
                        return true;
                    }
                    return false;
                }
            }
    
            public bool updateIngredient(int igId, string name, string unit, int price)
            {
                throw new NotImplementedException();
        }

    }
}
