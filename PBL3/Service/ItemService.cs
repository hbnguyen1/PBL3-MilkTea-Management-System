using PBL3.Data;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
                return true;
            }
            catch { return false; }
        }

        public bool AddItemWithRecipe(List<Item> items, List<Recipe> recipes, string localImagePath)
        {
            using (var transaction = _conn.Database.BeginTransaction())
            {
                try
                {
                    // 1. Lưu sản phẩm trước để SQL tự sinh ID
                    _conn.Items.Add(items[0]);
                    _conn.Items.Add(items[1]);
                    _conn.SaveChanges();

                    int realGeneratedItemId = items[0].itemID;

                    // 2. CHÉP ẢNH VÀ ĐỊNH DẠNG ĐƯỜNG DẪN CHUẨN KHOẢNG TRỐNG CỦA CSDL
                    if (!string.IsNullOrEmpty(localImagePath) && File.Exists(localImagePath))
                    {
                        // Thư mục mã nguồn gốc (Để Boss thấy trong Visual Studio)
                        string projectFolder = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                        string sourceImgFolder = Path.Combine(projectFolder, "Images");
                        if (!Directory.Exists(sourceImgFolder)) Directory.CreateDirectory(sourceImgFolder);

                        // Thư mục Debug (Để app load được ngay lập tức)
                        string debugImgFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                        if (!Directory.Exists(debugImgFolder)) Directory.CreateDirectory(debugImgFolder);

                        string ext = Path.GetExtension(localImagePath);
                        string fileNameOnly = $"mon_{realGeneratedItemId}{ext}";

                        // Copy file ảnh vào CẢ 2 NƠI
                        File.Copy(localImagePath, Path.Combine(sourceImgFolder, fileNameOnly), true);
                        File.Copy(localImagePath, Path.Combine(debugImgFolder, fileNameOnly), true);

                        // ĐÃ SỬA: Gán định dạng chuẩn có tiền tố /Images/ giống hệt DB cũ
                        string dbPath = $"/Images/{fileNameOnly}";
                        items[0].ImagePath = dbPath;
                        items[1].ImagePath = dbPath;

                        _conn.SaveChanges();
                    }

                    // 3. Lưu công thức
                    foreach (var recipe in recipes)
                    {
                        recipe.itemID = realGeneratedItemId;
                        _conn.Recipes.Add(recipe);
                    }

                    _conn.SaveChanges();
                    transaction.Commit();
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

        public Item? GetItemById(int itemId)
        {
            return _conn.Items.Include(i => i.Recipes).FirstOrDefault(i => i.itemID == itemId);
        }

        public Item? GetItemSize(int itemId, string size)
        {
            return _conn.Items.Include(i => i.Recipes).SingleOrDefault(i => i.itemID == itemId && i.size == size && i.isAvailable == true);
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
            if (recipe.Count == 0) return false;
            foreach (var item in recipe) { if (!_ingredientService.isAvailable(item.ingredientID, (int)item.quantityNeeded)) return false; }
            return true;
        }

        private bool CheckAvailabilityFromList(List<Recipe> itemRecipes, string size)
        {
            var specificRecipes = itemRecipes.Where(r => r.size == size).ToList();
            if (specificRecipes.Count == 0) return false;
            foreach (var recipe in specificRecipes) { if (!_ingredientService.isAvailable(recipe.ingredientID, (int)recipe.quantityNeeded)) return false; }
            return true;
        }

        public bool isAvailableWithCount(int itemId, string size, int quantity)
        {
            var recipe = _conn.Recipes.Where(r => r.itemID == itemId && r.size == size).ToList();
            if (recipe.Count == 0) return false;
            foreach (var item in recipe) { if (!_ingredientService.isAvailable(item.ingredientID, (int)(item.quantityNeeded * quantity))) return false; }
            return true;
        }

        public List<Item> GetMenuByCategory(string category)
        {
            var menu = _conn.Items.Include(i => i.Recipes).Where(i => i.itemType == category && i.size == "M" && i.isAvailable == true).ToList();
            foreach (var item in menu)
            {
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
            foreach (var item in recipe) { if (!_ingredientService.isAvailable(item.ingredientID, (int)(item.quantityNeeded * quantity))) return false; }
            foreach (var item in recipe) { _ingredientService.DeductStock(item.ingredientID, (int)(item.quantityNeeded * quantity)); }
            return true;
        }

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