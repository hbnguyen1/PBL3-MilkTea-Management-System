using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Query;
using PBL3.Interface;
using PBL3.Models;

namespace PBL3.Manangers
{
    internal class ItemManager
    {
        void AddItem()
        {
            string size = Console.ReadLine();
            string itemName = Console.ReadLine();
            string itemType = Console.ReadLine();
            int price = int.Parse(Console.ReadLine());
            var newItem = new Item
            {
                size = size,
                itemName = itemName,
                itemType = itemType,
                price = price,
                isAvailable = true
            };
            ItemService itemService = new ItemService();
            if (itemService.AddItem(newItem))
            {
                Console.WriteLine("Thêm sản phẩm thành công!");
            }
        }
        public void DeleteItemByID()
        {
            Console.WriteLine("Nhap id muon xoa: ");
            int id = int.Parse(Console.ReadLine());
            ItemService itemService = new ItemService();
            if (itemService.DeleteItemByID(id))
            {
                Console.WriteLine("Xóa sản phẩm thành công!");
            }
            else
            {
                Console.WriteLine("Không tìm thấy sản phẩm với id đã nhập!");
            }
        }
        public void DeleteByObject(Item item)
        {
            ItemService itemService = new ItemService();
            itemService.RemoveItem(item);
        }
        public Item? GetItem()
        {
            Console.WriteLine("Nhap id muon lay: ");
            int id = int.Parse(Console.ReadLine());
            ItemService itemService = new ItemService();
            var item = itemService.GetItemById(id);
            if (item != null)
            {
                return item;
            }
            else
            {
                Console.WriteLine("Không tìm thấy sản phẩm với id đã nhập!");
                return null;
            }
        }
        public void UpdateItem()
        {
            // throw Exception("Chưa hoàn thiện chức năng này, vui lòng quay lại sau!");
        }

        public void ShowMenu(bool showDetail = false)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ItemService itemService = new ItemService();

            var MenuMilkTea = itemService.GetMenuByCategory("Milk Tea");
            var MenuFruitTea = itemService.GetMenuByCategory("Fruit Tea");
            var MenuTopping = itemService.GetMenuByCategory("Topping");

            Console.WriteLine("===== MENU =====");

            foreach (var it in MenuMilkTea)
            {
                Console.WriteLine($"{it.itemID} : {it.itemName} : {(it.isAvailable ? "Còn" : "Hết")}");
            }

            foreach (var it in MenuFruitTea)
            {
                Console.WriteLine($"{it.itemID} : {it.itemName} : {(it.isAvailable ? "Còn" : "Hết")}");
            }

            foreach (var it in MenuTopping)
            {
                Console.WriteLine($"{it.itemID} : {it.itemName} : {(it.isAvailable ? "Còn" : "Hết")}");
            }
            if (showDetail)
            {
                Console.WriteLine("Nhập id sản phẩm muốn xem chi tiết:");
                int id = int.Parse(Console.ReadLine());
                ShowItemDetails(id);
            }
        }
            //Console.OutputEncoding = System.Text.Encoding.UTF8;
            ////string category = "Milk Tea";
            //ItemService itemService = new ItemService();
            //var MenuMilkTea = itemService.GetMenuByCategory("Milk Tea");
            //var MenuFruitTea = itemService.GetMenuByCategory("Fruit Tea");
            //var MenuTopping = itemService.GetMenuByCategory("Topping");
            //foreach (var it in MenuMilkTea)
            //{
            //    Console.WriteLine($"{it.itemID} : {it.itemName} : {it.isAvailable}");
            //}
            //Console.WriteLine("Nhập id sản phẩm muốn xem chi tiết: ");
            //int id = int.Parse(Console.ReadLine());
            //var itemDetails = itemService.GetItemSizeAndPrice(id);
            //foreach (var it in itemDetails)
            //{
            //    Console.WriteLine($"{it.itemName} : {it.size} : {it.price} : {it.isAvailable}");
            //}
        //}
        public void ShowItemDetails(int id)
        {
            ItemService itemService = new ItemService();
            var itemDetails = itemService.GetItemSizeAndPrice(id);
            foreach (var it in itemDetails)
            {
                Console.WriteLine($"{it.itemName} : {it.size} : {it.price} : {it.isAvailable}");
            }
        }
    }
}
