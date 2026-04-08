using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.Interface
{
    internal interface IItemService
    {
        bool AddItem(Item item);
        bool RemoveItem(Item item);
        bool UpdateItem(int itemId, Item item);
        bool DeleteItemByID(int itemId);
        Item? GetItemById(int itemId);
        Item? GetItemSize(int itemId, string size);
        bool isAvailable(int itemId, string size);
        bool isAvailableWithCount(int itemId, string size, int quantity);
        public List<Item> GetMenuByCategory(string category);
        public List<Item> GetItemSizeAndPrice(int itemId);
        bool DeductStock(int itemId, string size, int quantity);
    }
}
