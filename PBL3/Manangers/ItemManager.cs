using System.Collections.Generic;
using System.Linq;
using PBL3.Interface;
using PBL3.Models;

namespace PBL3.Manangers
{
    internal class ItemManager
    {
        private ItemService itemService = new ItemService();

        public bool AddItem(string itemName, string itemType, int basePrice, List<Recipe> baseRecipes, bool isAvailable = true)
        {
            int newId = itemService.GetNextItemID();

            List<Item> newItems = new List<Item>
            {
                new Item { itemID = newId, size = "M", itemName = itemName, itemType = itemType, price = basePrice, isAvailable = isAvailable },
                new Item { itemID = newId, size = "L", itemName = itemName, itemType = itemType, price = basePrice + 5000, isAvailable = isAvailable }
            };

            List<Recipe> finalRecipes = new List<Recipe>();
            foreach (var r in baseRecipes)
            {
                finalRecipes.Add(new Recipe { itemID = newId, size = "M", ingredientID = r.ingredientID, quantityNeeded = r.quantityNeeded, unitUsed = r.unitUsed });
                finalRecipes.Add(new Recipe { itemID = newId, size = "L", ingredientID = r.ingredientID, quantityNeeded = (int)(r.quantityNeeded * 1.5), unitUsed = r.unitUsed });
            }

            return itemService.AddItemWithRecipe(newItems, finalRecipes);
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

        public bool UpdateItem(int itemId, string size, string itemName, string itemType, int price, bool isAvailable)
        {
            var updatedItem = new Item
            {
                itemID = itemId,
                size = size,
                itemName = itemName,
                itemType = itemType,
                price = price,
                isAvailable = isAvailable
            };

            return itemService.UpdateItem(itemId, updatedItem);
        }

        public List<Item> GetAllMenuItems()
        {
            List<Item> allItems = new List<Item>();

            var milkTeas = itemService.GetMenuByCategory("Milk Tea");
            if (milkTeas != null) allItems.AddRange(milkTeas);

            var fruitTeas = itemService.GetMenuByCategory("Fruit Tea");
            if (fruitTeas != null) allItems.AddRange(fruitTeas);

            var topping = itemService.GetMenuByCategory("Topping");
            if (topping != null) allItems.AddRange(topping);

            var others = itemService.GetMenuByCategory("Món khác");
            if (others != null) allItems.AddRange(others);

            return allItems.OrderBy(item => item.itemID).ToList();
        }
    }
}