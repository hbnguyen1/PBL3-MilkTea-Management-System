using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using PBL3.Core;
using PBL3.Interface;

namespace PBL3.Service
{
    internal class ItemService : IItemService
    {
        private readonly MilkTeaDBContext _conn;
        private readonly IIngredientService _ingredientService;
        public ItemService(MilkTeaDBContext conn, IIngredientService ingredientService)
        {
            _conn = conn;
            _ingredientService = ingredientService;
        }

        public bool AddItem(List<Item> items)
        {
                _conn.Items.AddRange(items);
                _conn.SaveChanges();

                foreach (var item in items)
                {
                    Logger.Info($"Đã thêm sản phẩm: {item.itemName} - {item.size} - {item.itemType} - {item.price}");
                }
                return true;
        }

        public bool AddItemWithRecipe(List<Item> items, List<Recipe> recipes)
        {

                using (var transaction = _conn.Database.BeginTransaction())
                {
                    try
                    {
                        _conn.Items.AddRange(items);
                        _conn.SaveChanges();
                        int itemId = items[0].itemID;
                        foreach (var recipe in recipes)
                        {
                            recipe.itemID = itemId;
                        }
                        _conn.Recipes.AddRange(recipes);
                        _conn.SaveChanges();
                        transaction.Commit();
                        Logger.Info($"Đã thêm sản phẩm với công thức: {items[0].itemName} - {items[0].size} - {items[0].itemType} - {items[0].price}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Logger.Error("Lỗi khi thêm sản phẩm với công thức: " + ex.Message);
                        if (ex.InnerException != null)
                        {
                            Logger.Error("Chi tiết SQL: " + ex.InnerException.Message);
                        }
                        return false;
                    }
                }
        }

        public bool DeleteItemByID(int itemId)
        {
                var itemsToDelete = _conn.Items.Where(i => i.itemID == itemId).ToList();
                if (itemsToDelete.Count > 0)
                {
                    foreach (var i in itemsToDelete)
                    {
                        i.isAvailable = false;
                    }
                    _conn.SaveChanges();
                    return true;
                }
            return false;
        }

        public int GetNextItemID()
        {
                return (_conn.Items.Max(i => (int?)i.itemID) ?? 0) + 1;
        }

        public Item? GetItemById(int itemId)
        {
                return _conn.Items.FirstOrDefault(i => i.itemID == itemId);
        }

        public Item? GetItemSize(int itemId, string size)
        {
                var item = _conn.Items.SingleOrDefault(i => i.itemID == itemId && i.size == size && i.isAvailable == true);
                return item;
        }
        public bool UpdateItem(int itemId, Item item)
        {
                var updateItem = _conn.Items.SingleOrDefault(i => i.itemID == itemId && i.size == item.size);
                if (updateItem != null)
                {
                    updateItem.itemName = item.itemName;
                    updateItem.itemType = item.itemType;
                    updateItem.price = item.price;
                    updateItem.isAvailable = item.isAvailable;

                    _conn.SaveChanges();
                    return true;
                }
            return false;
        }

        public bool isAvailable(int itemId, string size)
        {
                var recipe = _conn.Recipes.Where(r => r.itemID == itemId && r.size == size).ToList();
                if (recipe.Count == 0)
                {
                    return false;
                }
                foreach (var item in recipe)
                {
                    if (!(_ingredientService.isAvailable(item.ingredientID, (int)item.quantityNeeded)))
                    {
                        return false;
                    }
                }
                return true;      
        }

        public bool isAvailableWithCount(int itemId, string size, int quantity)
        {
                var recipe = _conn.Recipes.Where(r => r.itemID == itemId && r.size == size).ToList();
                if (recipe.Count == 0)
                {
                    return false;
                }
                foreach (var item in recipe)
                {
                    if (!(_ingredientService.isAvailable(item.ingredientID, (int)(item.quantityNeeded * quantity))))
                    {
                        return false;
                    }
                }
                return true;
        }

        public List<Item> GetMenuByCategory(string category)
        {
                var menu = _conn.Items
                               .Where(i => i.itemType == category && i.size == "M" && i.isAvailable == true)
                               .ToList();
                foreach (var item in menu)
                {
                    bool sizeM = isAvailable(item.itemID, "M");
                    bool sizeL = isAvailable(item.itemID, "L");
                    item.isAvailable = sizeM || sizeL;
                }
                return menu;
        }

        public List<Item> GetItemSizeAndPrice(int itemId)
        {
                var itemList = _conn.Items.Where(i => i.itemID == itemId && i.isAvailable == true).ToList();
                foreach (var item in itemList)
                {
                    item.isAvailable = isAvailable(item.itemID, item.size);
                }
                return itemList;
        }

        public bool DeductStock(int itemId, string size, int quantity)
        {
                var recipe = _conn.Recipes.Where(r => r.itemID == itemId && r.size == size).ToList();
                if (recipe.Count == 0)
                {
                    return false;
                }
                foreach (var item in recipe)
                {
                    if (!(_ingredientService.isAvailable(item.ingredientID, (int)(item.quantityNeeded * quantity))))
                    {
                        return false;
                    }
                }
                foreach (var item in recipe)
                {
                    _ingredientService.DeductStock(item.ingredientID, (int)(item.quantityNeeded * quantity));
                }
                return true;
            }
        public List<Item> GetAllItems()
        {
            return _conn.Items.ToList();
        }
        public void UpdateItemWithRecipe(int itemId, Item mItem, Item lItem, List<Recipe> recipes)
        {
            using (var transaction = _conn.Database.BeginTransaction())
            {
                try
                {
                    var existingM = _conn.Items.FirstOrDefault(i => i.itemID == itemId && i.size == "M");
                    var existingL = _conn.Items.FirstOrDefault(i => i.itemID == itemId && i.size == "L");

                    if (existingM != null)
                    {
                        _conn.Entry(existingM).CurrentValues.SetValues(mItem);
                    }
                    if (existingL != null)
                    {
                        _conn.Entry(existingL).CurrentValues.SetValues(lItem);
                    }

                    var oldRecipes = _conn.Recipes.Where(r => r.itemID == itemId).ToList();
                    _conn.Recipes.RemoveRange(oldRecipes);
                    _conn.Recipes.AddRange(recipes);

                    _conn.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        public List<Recipe> GetRecipesByItem(int itemid, string size)
        { 
            return _conn.Recipes.Where(r => r.itemID == itemid && r.size == size).ToList();
        }
    }
}