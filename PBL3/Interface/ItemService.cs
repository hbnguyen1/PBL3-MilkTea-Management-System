using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Data;
using PBL3.Models;

namespace PBL3.Interface
{
    internal class ItemService : IItemService
    {
        public bool AddItem(Item item)
        {
            using (var conn = new MilkTeaDBContext())
            {
                conn.Items.Add(item);
                conn.SaveChanges();
                return true;
            }
            //return false;
        }
        public bool DeleteItemByID(int itemId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var item = conn.Items.Find(itemId);
                if (item != null)
                {
                    conn.Items.Remove(item);
                    conn.SaveChanges();
                    return true;
                }
            }
            return false;
        }
        public Item? GetItemById(int itemId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                return conn.Items.Find(itemId);
            }
        }
        public Item? GetItemSize(int itemId, string size)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var item = conn.Items.SingleOrDefault(i => i.itemID == itemId && i.size == size && i.isAvailable == true);
                return item;
            }
        }
        public bool RemoveItem(Item item)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var revItem = conn.Items.Find(item.itemID);
                if (revItem != null)
                {
                    conn.Items.Remove(revItem);
                    conn.SaveChanges();
                    return true;
                }
            }
            return false;
        }
        public bool UpdateItem(int itemId, Item item)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var updateItem = conn.Items.Find(itemId);
                if (updateItem != null)
                {
                    updateItem = item;
                    conn.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool isAvailable(int itemId, string size)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var recipe = conn.Recipes.Where(r => r.itemID == itemId && r.size == size).ToList();
                if (recipe.Count == 0)
                {
                    return false;
                }
                IngredientService ingredientService = new IngredientService();
                foreach (var item in recipe)
                {
                    if (!(ingredientService.isAvailable(item.ingredientID, item.quantityNeeded)))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool isAvailableWithCount(int itemId, string size, int quantity)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var recipe = conn.Recipes.Where(r => r.itemID == itemId && r.size == size).ToList();
                if (recipe.Count == 0)
                {
                    return false;
                }
                IngredientService ingredientService = new IngredientService();
                foreach (var item in recipe)
                {
                    if (!(ingredientService.isAvailable(item.ingredientID, item.quantityNeeded * quantity)))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public List<Item> GetMenuByCategory(string category)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var menu = conn.Items
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
        }
        public List<Item> GetItemSizeAndPrice(int itemId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var itemList = conn.Items.Where(i => i.itemID == itemId && i.isAvailable == true).ToList();
                foreach (var item in itemList)
                {
                    item.isAvailable = isAvailable(item.itemID, item.size);
                }
                return itemList;
            }
        }
        public bool DeductStock(int itemId, string size, int quantity)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var recipe = conn.Recipes.Where(r => r.itemID == itemId && r.size == size).ToList();
                if (recipe.Count == 0)
                {
                    return false;
                }
                IngredientService ingredientService = new IngredientService();
                foreach (var item in recipe)
                {
                    if (!(ingredientService.isAvailable(item.ingredientID, item.quantityNeeded * quantity)))
                    {
                        return false;
                    }
                }
                foreach (var item in recipe)
                {
                    ingredientService.DeductStock(item.ingredientID, item.quantityNeeded * quantity);
                }
                return true;
            }
        }
    }
}
