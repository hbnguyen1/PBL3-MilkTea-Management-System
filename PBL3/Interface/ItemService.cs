using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using PBL3.Core;

namespace PBL3.Interface
{
    internal class ItemService : IItemService
    {
        public bool AddItem(List<Item> items)
        {
            using (var conn = new MilkTeaDBContext())
            {
                conn.Items.AddRange(items);
                conn.SaveChanges();

                foreach (var item in items)
                {
                    Logger.Info($"Đã thêm sản phẩm: {item.itemName} - {item.size} - {item.itemType} - {item.price}");
                }

                return true;
            }
        }
        public bool isAvailableWithCount(int itemId, string size, int quantity)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var recipe = conn.Recipes
                    .Where(r => r.itemID == itemId && r.size == size)
                    .ToList();

                if (recipe.Count == 0)
                    return false;

                IngredientService ingredientService = new IngredientService();

                foreach (var item in recipe)
                {
                    if (!ingredientService.isAvailable(item.ingredientID, item.quantityNeeded * quantity))
                        return false;
                }

                return true;
            }
        }
        public bool AddItemWithRecipe(List<Item> items, List<Recipe> recipes)
        {
            using (var conn = new MilkTeaDBContext())
            {
                using (var transaction = conn.Database.BeginTransaction())
                {
                    try
                    {
                        conn.Items.AddRange(items);
                        conn.SaveChanges();

                        int itemId = items[0].itemID;

                        foreach (var recipe in recipes)
                        {
                            recipe.itemID = itemId;
                        }

                        conn.Recipes.AddRange(recipes);
                        conn.SaveChanges();

                        transaction.Commit();

                        Logger.Info($"Đã thêm sản phẩm với công thức: {items[0].itemName}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        Logger.Error("Lỗi khi thêm sản phẩm: " + ex.Message);
                        if (ex.InnerException != null)
                            Logger.Error("SQL: " + ex.InnerException.Message);

                        return false;
                    }
                }
            }
        }
        public bool DisableItemById(int itemId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var item = conn.Items.Find(itemId);
                if (item != null)
                {
                    item.isAvailable = false;
                    return true;
                }
            }
            return false;
        }

        public int GetNextItemID()
        {
            using (var db = new MilkTeaDBContext())
            {
                return (db.Items.Max(i => (int?)i.itemID) ?? 0) + 1;
            }
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
                return conn.Items
                    .SingleOrDefault(i => i.itemID == itemId && i.size == size && i.isAvailable == true);
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
                    updateItem.itemName = item.itemName;
                    updateItem.price = item.price;
                    updateItem.size = item.size;
                    updateItem.itemType = item.itemType;
                    updateItem.isAvailable = item.isAvailable;

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
                var recipe = conn.Recipes
                    .Where(r => r.itemID == itemId && r.size == size)
                    .ToList();

                if (recipe.Count == 0)
                    return false;

                IngredientService ingredientService = new IngredientService();

                foreach (var item in recipe)
                {
                    if (!ingredientService.isAvailable(item.ingredientID, item.quantityNeeded))
                        return false;
                }

                return true;
            }
        }
        private Dictionary<(int, string), bool> CheckAvailabilityBulk(List<(int itemId, string size)> requests)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var result = new Dictionary<(int, string), bool>();

                var itemIds = requests.Select(r => r.itemId).Distinct().ToList();

                var recipes = conn.Recipes
                    .Where(r => itemIds.Contains(r.itemID))
                    .ToList();

                IngredientService ingredientService = new IngredientService();

                foreach (var req in requests)
                {
                    var recipeList = recipes
                        .Where(r => r.itemID == req.itemId && r.size == req.size)
                        .ToList();

                    if (recipeList.Count == 0)
                    {
                        result[(req.itemId, req.size)] = false;
                        continue;
                    }

                    bool ok = true;

                    foreach (var r in recipeList)
                    {
                        if (!ingredientService.isAvailable(r.ingredientID, r.quantityNeeded))
                        {
                            ok = false;
                            break;
                        }
                    }

                    result[(req.itemId, req.size)] = ok;
                }

                return result;
            }
        }

        public List<Item> GetMenuByCategory(string category)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var menu = conn.Items
                    .Where(i => i.itemType == category && i.size == "M" && i.isAvailable == true)
                    .ToList();

                var requests = new List<(int, string)>();

                foreach (var item in menu)
                {
                    requests.Add((item.itemID, "M"));
                    requests.Add((item.itemID, "L"));
                }

                var availability = CheckAvailabilityBulk(requests);

                foreach (var item in menu)
                {
                    bool sizeM = availability[(item.itemID, "M")];
                    bool sizeL = availability[(item.itemID, "L")];
                    item.isAvailable = sizeM || sizeL;
                }

                return menu;
            }
        }

        public List<Item> GetItemSizeAndPrice(int itemId)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var items = conn.Items
                    .Where(i => i.itemID == itemId && i.isAvailable == true)
                    .ToList();

                var requests = items
                    .Select(i => (i.itemID, i.size))
                    .ToList();

                var availability = CheckAvailabilityBulk(requests);

                foreach (var item in items)
                {
                    item.isAvailable = availability[(item.itemID, item.size)];
                }

                return items;
            }
        }

        public bool DeductStock(int itemId, string size, int quantity)
        {
            using (var conn = new MilkTeaDBContext())
            {
                var recipe = conn.Recipes
                    .Where(r => r.itemID == itemId && r.size == size)
                    .ToList();

                if (recipe.Count == 0)
                    return false;

                IngredientService ingredientService = new IngredientService();

                foreach (var item in recipe)
                {
                    if (!ingredientService.isAvailable(item.ingredientID, item.quantityNeeded * quantity))
                        return false;
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