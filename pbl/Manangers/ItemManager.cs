using System.Collections.Generic;
using PBL3.Interface;
using PBL3.Models;

namespace PBL3.Manangers
{
    internal class ItemManager
    {
        private ItemService itemService = new ItemService();

        public bool AddItem(string size, string itemName, string itemType, int price, bool isAvailable = true)
        {
            var newItem = new Item
            {
                size = size,
                itemName = itemName,
                itemType = itemType,
                price = price,
                isAvailable = isAvailable
            };
            return itemService.AddItem(newItem);
        }

        public bool DeleteItemByID(int id)
        {
            return itemService.DeleteItemByID(id);
        }

        public void DeleteByObject(Item item)
        {
            itemService.RemoveItem(item);
        }

        public Item? GetItem(int id)
        {
            return itemService.GetItemById(id);
        }

        public List<Item> GetAllMenuItems()
        {
            List<Item> allItems = new List<Item>();

            // Lấy danh sách Trà sữa
            var milkTeas = itemService.GetMenuByCategory("Milk Tea");
            if (milkTeas != null) allItems.AddRange(milkTeas);

            // Lấy danh sách Trà trái cây
            var fruitTeas = itemService.GetMenuByCategory("Fruit Tea");
            if (fruitTeas != null) allItems.AddRange(fruitTeas);

            // Lấy danh sách Topping (nếu muốn)
            // var toppings = itemService.GetMenuByCategory("Topping");
            // if (toppings != null) allItems.AddRange(toppings);

            return allItems;
        }
    }
}