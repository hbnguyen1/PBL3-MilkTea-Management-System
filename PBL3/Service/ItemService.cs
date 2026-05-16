using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using PBL3.Core;
using PBL3.Interface;
using Microsoft.EntityFrameworkCore;

namespace PBL3.Service
{
    internal class ItemService : IItemService
    {
        private readonly MilkTeaDBContext _conn;
        private readonly IIngredientService _ingredientService;
        private static readonly object _idLock = new object();

        public ItemService(MilkTeaDBContext conn, IIngredientService ingredientService)
        {
            _conn = conn;
            _ingredientService = ingredientService;
        }

        public bool AddItem(List<Item> items)
        {
            try
            {
                _conn.Items.AddRange(items);
                _conn.SaveChanges();
                foreach (var item in items) Logger.Info($"Đã thêm: {item.itemName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi AddItem: {ex.Message}");
                return false;
            }
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
                    foreach (var recipe in recipes) recipe.itemID = itemId;

                    _conn.Recipes.AddRange(recipes);
                    _conn.SaveChanges();
                    transaction.Commit();
                    Logger.Info($"Đã thêm SP + CT: {items[0].itemName}");
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error("Lỗi AddItemWithRecipe: " + ex.Message);
                    return false;
                }
            }
        }

        public bool DeleteItemByID(int itemId)
        {
            var itemsToDelete = _conn.Items.Where(i => i.itemID == itemId).ToList();
            if (itemsToDelete.Count > 0)
            {
                foreach (var i in itemsToDelete) i.isAvailable = false;
                _conn.SaveChanges();
                return true;
            }
            return false;
        }

        public int GetNextItemID()
        {
            lock (_idLock)
            {
                int maxId = _conn.Items.Max(i => (int?)i.itemID) ?? 0;
                return maxId + 1;
            }
        }

        //Thêm Include() khi lấy Item đơn lẻ
        public Item? GetItemById(int itemId)
        {
            return _conn.Items
                        .Include(i => i.Recipes) // Lấy kèm công thức
                        .FirstOrDefault(i => i.itemID == itemId);
        }

        public Item? GetItemSize(int itemId, string size)
        {
            return _conn.Items
                        .Include(i => i.Recipes)
                        .SingleOrDefault(i => i.itemID == itemId && i.size == size && i.isAvailable == true);
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

        //Nhận luôn list recipes có sẵn, không gọi xuống DB nữa
        public bool isAvailable(int itemId, string size)
        {
            // Tránh N+1 bằng cách lấy trực tiếp từ DB nếu lỡ gọi lẻ
            var recipe = _conn.Recipes.Where(r => r.itemID == itemId && r.size == size).ToList();
            if (recipe.Count == 0) return false;

            foreach (var item in recipe)
            {
                if (!_ingredientService.isAvailable(item.ingredientID, (int)item.quantityNeeded)) return false;
            }
            return true;
        }

        // Hàm mới bổ trợ: Kiểm tra dựa trên list Recipes có sẵn trên RAM (Không gọi DB)
        private bool CheckAvailabilityFromList(List<Recipe> itemRecipes, string size)
        {
            var specificRecipes = itemRecipes.Where(r => r.size == size).ToList();
            if (specificRecipes.Count == 0) return false;

            foreach (var recipe in specificRecipes)
            {
                if (!_ingredientService.isAvailable(recipe.ingredientID, (int)recipe.quantityNeeded)) return false;
            }
            return true;
        }

        public bool isAvailableWithCount(int itemId, string size, int quantity)
        {
            var recipe = _conn.Recipes.Where(r => r.itemID == itemId && r.size == size).ToList();
            if (recipe.Count == 0) return false;

            foreach (var item in recipe)
            {
                if (!_ingredientService.isAvailable(item.ingredientID, (int)(item.quantityNeeded * quantity))) return false;
            }
            return true;
        }

        public List<Item> GetMenuByCategory(string category)
        {
            // 1. Dùng Include để JOIN bảng Items và Recipes trong 1 query duy nhất
            var menu = _conn.Items
                           .Include(i => i.Recipes)
                           .Where(i => i.itemType == category && i.size == "M" && i.isAvailable == true)
                           .ToList();

            // 2. Lặp qua danh sách trên RAM, không chọc xuống DB nữa
            foreach (var item in menu)
            {
                // Truy cập i.Recipes không tốn thêm query vì đã Include ở trên
                bool sizeM = CheckAvailabilityFromList(item.Recipes.ToList(), "M");
                bool sizeL = CheckAvailabilityFromList(item.Recipes.ToList(), "L");
                item.isAvailable = sizeM || sizeL;
            }
            return menu;
        }

        public List<Item> GetItemSizeAndPrice(int itemId)
        {
            return _conn.Items.Where(i => i.itemID == itemId && i.isAvailable == true).ToList();
        }

        public bool DeductStock(int itemId, string size, int quantity)
        {
            var recipe = _conn.Recipes.Where(r => r.itemID == itemId && r.size == size).ToList();
            if (recipe.Count == 0) return false;

            foreach (var item in recipe)
            {
                if (!_ingredientService.isAvailable(item.ingredientID, (int)(item.quantityNeeded * quantity))) return false;
            }

            foreach (var item in recipe)
            {
                _ingredientService.DeductStock(item.ingredientID, (int)(item.quantityNeeded * quantity));
            }
            return true;
        }

        //GetAll cũng gom luôn Recipe
        public List<Item> GetAllItems()
        {
            return _conn.Items.Include(i => i.Recipes).ToList();
        }

        public void UpdateItemWithRecipe(int itemId, Item mItem, Item lItem, List<Recipe> recipes)
        {
            using (var transaction = _conn.Database.BeginTransaction())
            {
                try
                {
                    var existingM = _conn.Items.FirstOrDefault(i => i.itemID == itemId && i.size == "M");
                    var existingL = _conn.Items.FirstOrDefault(i => i.itemID == itemId && i.size == "L");

                    if (existingM != null) _conn.Entry(existingM).CurrentValues.SetValues(mItem);
                    if (existingL != null) _conn.Entry(existingL).CurrentValues.SetValues(lItem);

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