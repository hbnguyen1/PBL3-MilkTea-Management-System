using System;
using System.Collections.Generic;
using System.Text;
using PBL3.Interface;
using PBL3.Models;

namespace PBL3.Manangers
{
    internal class IngredientManager
    {
        public bool AddIngredient()
        {
            Console.WriteLine("Nhập tên nguyên liệu: ");
            Console.WriteLine("Nhập giá nguyên liệu:");
            Console.WriteLine("Nhập đơn vị nguyên liệu:");
            try
            {
                string name = Console.ReadLine();
                int price = int.Parse(Console.ReadLine());
                string unit = Console.ReadLine();
                return true;
            }
            catch (FormatException)
            {
                Console.WriteLine("Chưa nhập gì vui lòng nhập");
                return false;
            }
        }
        public void ShowAllIngredient()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            IngredientService ingredientService = new IngredientService();
            var list = ingredientService.GetAllIngredients();
            foreach (var item in list)
            {
                Console.WriteLine($"ID: {item.igID}, Name: {item.igName}, Price: {item.igCount}, Unit: {item.price}, Stock: {item.unit}");
            }
        }
        //public void ShowLowStockIngredients()
        //{
        //    Console.OutputEncoding = System.Text.Encoding.UTF8;
        //    IngredientService ingredientService = new IngredientService();
        //    var list = ingredientService.GetLowStockIngredients();
        //    foreach (var item in list)
        //    {
        //        Console.WriteLine($"ID: {item.igID}, Name: {item.igName}, Price: {item.igCount}, Unit: {item.price}, Stock: {item.unit}");
        //    }
        //}
    }
}
