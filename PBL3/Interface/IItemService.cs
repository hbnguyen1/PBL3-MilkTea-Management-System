using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    public interface IItemService
    {
        bool AddItem(List<Item> items);
        bool AddItemWithRecipe(List<Item> items, List<Recipe> recipes);
        bool UpdateItem(int itemId, Item item);
        bool DeleteItemByID(int itemId);
        int GetNextItemID();
        Item? GetItemById(int itemId);
        Item? GetItemSize(int itemId, string size);
        bool isAvailable(int itemId, string size);
        bool isAvailableWithCount(int itemId, string size, int quantity);
        public List<Item> GetMenuByCategory(string category);
        public List<Item> GetItemSizeAndPrice(int itemId);
        bool DeductStock(int itemId, string size, int quantity);
        List<Item> GetAllItems();
        void UpdateItemWithRecipe(int itemId, Item mItem, Item lItem, List<Recipe> recipes);
        List<Recipe> GetRecipesByItem(int itemId, string size);
    }
}
