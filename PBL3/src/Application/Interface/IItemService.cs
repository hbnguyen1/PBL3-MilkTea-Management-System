using System;
using System.Collections.Generic;
using PBL3.src.Domain.Models;

namespace PBL3.src.Application.Interface
{
    public interface IItemService
    {
        bool AddItem(List<Item> items);
        bool AddItemWithRecipe(List<Item> items, List<Recipe> recipes, string localImagePath);
        bool DeleteItemByID(int itemId);
        int GetNextItemID();
        Item? GetItemById(int itemId);
        Item? GetItemSize(int itemId, string size);
        bool UpdateItem(int itemId, Item item);
        bool isAvailable(int itemId, string size);
        bool isAvailableWithCount(int itemId, string size, int quantity);
        List<Item> GetMenuByCategory(string category);
        List<Item> GetItemSizeAndPrice(int itemId);
        bool DeductStock(int itemId, string size, int quantity);
        List<Item> GetAllItems();
        void UpdateItemWithRecipe(int itemId, Item mItem, Item lItem, List<Recipe> recipes);
        List<Recipe> GetRecipesByItem(int itemid, string size);
    }
}